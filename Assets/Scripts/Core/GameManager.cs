using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public int score = 0;
    public int lives = 3;
    public bool isPowerMode = false;
    public float powerModeTimer = 0f;
    public const float POWER_MODE_DURATION = 8f;

    [Header("Collision Settings")]
    public float collisionCheckRadius = 0.4f;

    [Header("Level Info")]
    [SerializeField] private string currentLevelName = "Level 1";
    [SerializeField] private string startSceneName = "StartScene";

    private float gameTimerSeconds = 0f;
    private bool gameTimerRunning = false;

    public float GameTimerSeconds => gameTimerSeconds;
    public string CurrentLevelName => currentLevelName;


    [Header("References")]
    private GameObject player;
    private List<GameObject> ghosts = new List<GameObject>();
    private GameObject levelParent;
    private Camera mainCamera;

    [Header("Runtime Sprites")]
    private Sprite wallSprite;
    private Sprite pelletSprite;
    private Sprite powerPelletSprite;
    private Sprite ghostHouseSprite;
    private Sprite entitySprite;

    private readonly Dictionary<Vector2Int, GameObject> pelletLookup = new Dictionary<Vector2Int, GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeGame();
    }

    void Update()
    {
         if (gameTimerRunning)
    {
        gameTimerSeconds += Time.deltaTime;
    }

    if (isPowerMode)
    {
        powerModeTimer -= Time.deltaTime;
        if (powerModeTimer <= 0f)
        {
            DeactivatePowerMode();
        }
    }

  
    if (Input.GetKeyDown(KeyCode.R) && Time.timeScale != 0f)
    {
        RestartGame();
    }
    
    CheckCollisions();
    }


    void CheckCollisions()
    {
        if (player == null) return;

        Vector3 playerPos = player.transform.position;

        foreach (GameObject ghost in ghosts)
        {
            if (ghost == null) continue;

            float distance = Vector3.Distance(playerPos, ghost.transform.position);

            if (distance < collisionCheckRadius)
            {
                GhostController ghostController = ghost.GetComponent<GhostController>();

                if (ghostController.IsVulnerable())
                {
                    // 幽灵可被吃掉
                    EatGhost(ghostController);
                }
                else if (ghostController.currentMode != GhostMode.Dead)
                {
                    // 玩家被幽灵碰到
                    PlayerHit();
                }
            }
        }
    }

    void InitializeGame()
    {
        Debug.Log("=== Initializing Game ===");

        // 创建核心系统
        CreateCamera();
        CreateGridManager();
        PrepareSprites();
        CreateLevel();
        CreatePlayer();
        CreateGhosts();
        CreateUI();

        Debug.Log("=== Game Initialization Complete ===");
        Debug.Log("Controls: WASD or Arrow Keys to move");
        Debug.Log("Press R to restart");

        GameObject audioObj = new GameObject("Audio Manager");
        audioObj.transform.SetParent(transform);
        audioObj.AddComponent<AudioManager>();

        gameTimerSeconds = 0f;
        gameTimerRunning = true;

        Debug.Log("=== Game Initialization Complete ===");
    }


    void PrepareSprites()
    {
        wallSprite = SpriteFactory.CreateRoundedRectSprite(
            new Color(0.15f, 0.2f, 0.9f),
            new Color(0.05f, 0.08f, 0.4f),
            64,
            64,
            borderSize: 6,
            cornerRadius: 10
        );

        pelletSprite = SpriteFactory.CreateCircleSprite(new Color(1f, 0.85f, 0.6f), 32);
        powerPelletSprite = SpriteFactory.CreateCircleSprite(Color.white, 48);
        ghostHouseSprite = SpriteFactory.CreateRoundedRectSprite(
            new Color(1f, 0.75f, 0.85f, 0.35f),
            new Color(1f, 0.55f, 0.75f, 0.45f),
            64,
            64,
            borderSize: 4,
            cornerRadius: 16
        );
        entitySprite = SpriteFactory.CreateCircleSprite(Color.white, 64);
    }


    void CreateCamera()
    {
        GameObject camObj = new GameObject("Main Camera");
        mainCamera = camObj.AddComponent<Camera>();
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 11f;
        mainCamera.backgroundColor = Color.black;
        camObj.tag = "MainCamera";
        camObj.AddComponent<AudioListener>();

        // 居中摄像机
        int[,] level = LevelData.GetLevel1();
        float centerX = (level.GetLength(1) - 1) / 2f;
        float centerY = (level.GetLength(0) - 1) / 2f;
        camObj.transform.position = new Vector3(centerX, centerY, -10f);

        Debug.Log($"Camera created at position: ({centerX}, {centerY}, -10)");
    }


    void CreateGridManager()
    {
        GameObject gridObj = new GameObject("Grid Manager");
        gridObj.transform.SetParent(transform);
        GridManager gridManager = gridObj.AddComponent<GridManager>();

        // 初始化网格数据
        int[,] levelData = LevelData.GetLevel1();
        gridManager.InitializeGrid(levelData);

        Debug.Log("Grid Manager created");
    }

    void EatGhost(GhostController ghost)
    {
        int points = 200;
        AddScore(points);
        ghost.Die();

        AudioManager.Instance?.PlaySound("GhostEaten");
        Debug.Log($"Ghost eaten! +{points} points");
    }

    void CreateLevel()
    {
        levelParent = new GameObject("=== LEVEL ===");
        levelParent.transform.SetParent(transform);
        pelletLookup.Clear();

        int[,] levelData = LevelData.GetLevel1();
        int height = levelData.GetLength(0);
        int width = levelData.GetLength(1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 pos = new Vector3(x, y, 0);
                int cellType = levelData[y, x];

                switch (cellType)
                {
                    case GridManager.WALL:
                        CreateWall(pos, x, y);
                        break;
                    case GridManager.PELLET:
                    case GridManager.EMPTY:
                        CreatePellet(pos, false, new Vector2Int(x, y));
                        break;
                    case GridManager.POWER_PELLET:
                        CreatePellet(pos, true, new Vector2Int(x, y));
                        break;
                    case GridManager.GHOST_HOUSE:
                        CreateGhostHouse(pos);
                        break;
                }
            }
        }

        Debug.Log($"Level created: {width}x{height}");
    }


    void CreateWall(Vector3 position, int x, int y)
    {
        GameObject wall = new GameObject($"Wall_{x}_{y}");
        wall.transform.SetParent(levelParent.transform);
        wall.transform.position = position;

        SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = wallSprite;
        sr.sortingOrder = -2;
    }


    void CreatePellet(Vector3 position, bool isPowerPellet, Vector2Int gridPosition)
    {
        GameObject pellet = new GameObject(isPowerPellet ? $"PowerPellet_{gridPosition.x}_{gridPosition.y}" : $"Pellet_{gridPosition.x}_{gridPosition.y}");
        pellet.transform.SetParent(levelParent.transform);
        pellet.transform.position = position;

        SpriteRenderer sr = pellet.AddComponent<SpriteRenderer>();
        sr.sprite = isPowerPellet ? powerPelletSprite : pelletSprite;
        sr.sortingOrder = 0;
        sr.color = isPowerPellet ? Color.white : Color.white;

        float scale = isPowerPellet ? 0.75f : 0.35f;
        pellet.transform.localScale = Vector3.one * scale;

        if (isPowerPellet)
        {
            pellet.AddComponent<PowerPelletBlink>();
        }

        pelletLookup[gridPosition] = pellet;
    }


    void CreateGhostHouse(Vector3 position)
    {
        GameObject house = new GameObject("GhostHouse");
        house.transform.SetParent(levelParent.transform);
        house.transform.position = position;

        SpriteRenderer sr = house.AddComponent<SpriteRenderer>();
        sr.sprite = ghostHouseSprite;
        sr.sortingOrder = -1;
    }


    void CreatePlayer()
    {
        player = new GameObject("PacStudent");
        player.transform.SetParent(transform);

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = entitySprite;
        sr.color = new Color(1f, 0.9f, 0.1f);
        sr.sortingOrder = 5;

        player.transform.localScale = Vector3.one * 0.9f;

        player.AddComponent<PacStudentController>();

        Debug.Log("Player created");
    }


    void CreateGhosts()
    {
        string[] ghostNames = { "Blinky", "Pinky", "Inky", "Clyde" };
        Color[] ghostColors = {
            Color.red,
            new Color(1f, 0.75f, 0.8f),
            Color.cyan,
            new Color(1f, 0.7f, 0.1f)
        };
        Vector2Int[] positions = LevelData.GetGhostStartPositions();

        for (int i = 0; i < ghostNames.Length; i++)
        {
            GameObject ghost = new GameObject(ghostNames[i]);
            ghost.transform.SetParent(transform);

            SpriteRenderer sr = ghost.AddComponent<SpriteRenderer>();
            sr.sprite = entitySprite;
            sr.color = ghostColors[i];
            sr.sortingOrder = 4;

            ghost.transform.localScale = Vector3.one * 0.85f;
            ghost.transform.position = GridManager.Instance.GridToWorld(positions[i]);

            GhostController controller = ghost.AddComponent<GhostController>();
            controller.ghostType = (GhostType)i;

            CreateGhostLabel(ghost, i + 1);
            ghosts.Add(ghost);
        }

        Debug.Log($"Created {ghosts.Count} ghosts");
    }


    void CreateGhostLabel(GameObject ghost, int labelNumber)
    {
        GameObject canvasObj = new GameObject($"Ghost{labelNumber}Canvas");
        canvasObj.transform.SetParent(ghost.transform);
        canvasObj.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        canvasObj.transform.localRotation = Quaternion.identity;

        Canvas canvasComponent = canvasObj.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.WorldSpace;
        canvasComponent.worldCamera = mainCamera;
        canvasComponent.sortingOrder = 15;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 32f;

        canvasObj.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvasComponent.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1.5f, 0.8f);
        canvasRect.localScale = Vector3.one * 0.1f;

        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(canvasObj.transform);

        Text label = textObj.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 80;
        label.alignment = TextAnchor.MiddleCenter;
        label.text = labelNumber.ToString();
        label.color = Color.white;

        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(0.08f, -0.08f);

        RectTransform textRect = label.rectTransform;
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(1f, 0.5f);
    }


    void CreateUI()
    {
        GameObject uiObj = new GameObject("=== UI ===");
        uiObj.transform.SetParent(transform);
        uiObj.AddComponent<GameUI>();

        Debug.Log("UI created");
    }


    public void AddScore(int points)
    {
        score += points;
        Debug.Log($"Score: {score}");
    }


    public void RemovePellet(Vector2Int gridPosition)
    {
        if (pelletLookup.TryGetValue(gridPosition, out GameObject pellet))
        {
            if (pellet != null)
            {
                Destroy(pellet);
            }
            pelletLookup.Remove(gridPosition);
        }
    }


    public void ActivatePowerMode()
    {
        isPowerMode = true;
        powerModeTimer = POWER_MODE_DURATION;

        AudioManager.Instance?.PlayPowerModeMusic();

        foreach (GameObject ghost in ghosts)
        {
            GhostController controller = ghost.GetComponent<GhostController>();
            if (controller != null)
            {
                controller.EnterFrightenedMode();
            }
        }

        Debug.Log("POWER MODE ACTIVATED!");
    }


    void DeactivatePowerMode()
    {
        isPowerMode = false;

        AudioManager.Instance?.StopPowerModeMusic();

        foreach (GameObject ghost in ghosts)
        {
            GhostController controller = ghost.GetComponent<GhostController>();
            if (controller != null)
            {
                controller.ExitFrightenedMode();
            }
        }

        Debug.Log("Power mode ended");
    }


    /// <summary>
    /// 重启游戏 - 方案2：直接重新初始化
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("🔴 ========== RESTART GAME CALLED ==========");
    Debug.Log($"Current timeScale: {Time.timeScale}");
    
    // 停止所有协程
    StopAllCoroutines();
    Debug.Log("✅ All coroutines stopped");
    
    // 立即恢复时间
    Time.timeScale = 1f;
    Debug.Log("✅ timeScale set to 1");
    
    // 开始重启流程
    Debug.Log("🟡 Starting PerformRestart coroutine...");
    StartCoroutine(PerformRestart());
    }
    
    IEnumerator PerformRestart()
{
    Debug.Log("🟢 PerformRestart: Started");
    
    // 销毁 Game Over UI
    GameObject gameOverUI = GameObject.Find("GameOverUI");
    if (gameOverUI != null)
    {
        Debug.Log("🟡 Found GameOverUI, destroying...");
        Destroy(gameOverUI);
    }
    else
    {
        Debug.Log("⚠️ GameOverUI not found");
    }
    
    // 查找并销毁 Game Over Canvas
    GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
    foreach (GameObject obj in allObjects)
    {
        if (obj.name.Contains("GameOver") || obj.name.Contains("Canvas"))
        {
            Debug.Log($"🟡 Destroying: {obj.name}");
            Destroy(obj);
        }
    }
    
    Debug.Log("🟢 Cleaning up child objects...");
    
    // 清理所有子对象
    int childCount = transform.childCount;
    Debug.Log($"🟡 Found {childCount} child objects");
    
    for (int i = transform.childCount - 1; i >= 0; i--)
    {
        Transform child = transform.GetChild(i);
        Debug.Log($"🟡 Destroying child: {child.name}");
        Destroy(child.gameObject);
    }
    
    // 清理玩家
    if (player != null)
    {
        Debug.Log("🟡 Destroying player");
        Destroy(player);
        player = null;
    }
    
    // 清理摄像机
    if (mainCamera != null)
    {
        Debug.Log("🟡 Destroying camera");
        Destroy(mainCamera.gameObject);
        mainCamera = null;
    }
    
    // 清空集合
    Debug.Log($"🟡 Clearing ghosts list (count: {ghosts.Count})");
    ghosts.Clear();
    
    Debug.Log($"🟡 Clearing pellet lookup (count: {pelletLookup.Count})");
    pelletLookup.Clear();
    
    levelParent = null;
    
    // 重置游戏状态
    score = 0;
    lives = 3;
    isPowerMode = false;
    powerModeTimer = 0f;
    gameTimerSeconds = 0f;
    gameTimerRunning = false;
    
    Debug.Log("🟢 All cleanup complete, waiting 0.1 seconds...");
    
    // 等待销毁完成
    yield return new WaitForSecondsRealtime(0.1f);
    
    Debug.Log("🟢 Wait complete, starting InitializeGame()...");
    
    // 重新初始化
    try
    {
        InitializeGame();
        Debug.Log("✅ ========== GAME RESTARTED SUCCESSFULLY ==========");
    }
    catch (System.Exception e)
    {
        Debug.LogError($"❌ ERROR during InitializeGame: {e.Message}");
        Debug.LogError($"Stack trace: {e.StackTrace}");
    }
}

    void ClearCurrentGame()
    {
        Debug.Log("Clearing current game objects...");

        // 销毁关卡
        if (levelParent != null)
        {
            Destroy(levelParent);
            levelParent = null;
        }

        // 销毁玩家
        if (player != null)
        {
            Destroy(player);
            player = null;
        }

        // 销毁所有幽灵
        foreach (GameObject ghost in ghosts)
        {
            if (ghost != null)
            {
                Destroy(ghost);
            }
        }
        ghosts.Clear();

        // 销毁 UI
        GameObject uiObj = GameObject.Find("=== UI ===");
        if (uiObj != null)
        {
            Destroy(uiObj);
        }

        // 销毁摄像机
        if (mainCamera != null)
        {
            Destroy(mainCamera.gameObject);
            mainCamera = null;
        }

        // 销毁网格管理器
        GameObject gridObj = GameObject.Find("Grid Manager");
        if (gridObj != null)
        {
            Destroy(gridObj);
        }

        // 销毁音频管理器（重置音乐）
        GameObject audioObj = GameObject.Find("Audio Manager");
        if (audioObj != null)
        {
            Destroy(audioObj);
        }

        // 清空豆子字典
        pelletLookup.Clear();

        Debug.Log("Game objects cleared");
    }


    public void ExitToStartScene()
    {
        gameTimerRunning = false;
        Time.timeScale = 1f;

        if (string.IsNullOrEmpty(startSceneName))
        {
            Debug.LogWarning("Start scene name is not configured.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(startSceneName))
        {
            Debug.LogWarning($"Unable to load start scene '{startSceneName}'. Check build settings.");
            return;
        }

        if (SceneManager.GetActiveScene().name == startSceneName)
        {
            Debug.Log("Already in start scene.");
            return;
        }

        SceneManager.LoadScene(startSceneName);
    }

    public Vector2Int GetPlayerGridPosition()
    {
        if (player != null)
        {
            PacStudentController controller = player.GetComponent<PacStudentController>();
            if (controller != null)
            {
                return controller.GetGridPosition();
            }
        }
        return Vector2Int.zero;
    }

    public Vector2Int GetPlayerDirection()
    {
        if (player != null)
        {
            PacStudentController controller = player.GetComponent<PacStudentController>();
            if (controller != null)
            {
                return controller.GetCurrentDirection();
            }
        }
        return Vector2Int.zero;
    }

    void PlayerHit()
    {
        lives--;
        AudioManager.Instance?.PlaySound("PlayerDeath");

        Debug.Log($"Player hit! Lives remaining: {lives}");

        if (lives <= 0)
        {
            GameOver();
        }
        else
        {
            RespawnPlayer();
        }
    }


    void RespawnPlayer()
    {
        if (player != null)
        {
            Vector2Int startPos = LevelData.GetPlayerStartPosition();
            player.transform.position = GridManager.Instance.GridToWorld(startPos);

            // 重置玩家状态
            PacStudentController controller = player.GetComponent<PacStudentController>();
            controller.ResetPosition();

            // 暂停游戏 2 秒
            Time.timeScale = 0f;
            gameTimerRunning = false;
            StartCoroutine(ResumeAfterDelay(2f));
        }
    }


    IEnumerator ResumeAfterDelay(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        Time.timeScale = 1f;
        gameTimerRunning = true;
    }


    void GameOver()
    {
        Debug.Log("=== GAME OVER ===");
        Time.timeScale = 0f;
        gameTimerRunning = false;

        // 显示游戏结束界面
        GameObject gameOverUI = new GameObject("GameOverUI");
        gameOverUI.AddComponent<GameOverUI>();

        AudioManager.Instance?.PlaySound("GameOver");
        AudioManager.Instance?.StopMusic();
    }
}