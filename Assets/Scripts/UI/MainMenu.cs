using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    private Canvas canvas;
    private int highScore = 0;
    private bool gameStarted = false;

    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);

       
        CreateEventSystem();

        CreateMenuUI();

 
        Debug.Log("Main Menu initialized - Button should work now");
    }

 
    void CreateEventSystem()
    {
        
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();

            Debug.Log("? EventSystem created");
        }
    }

    void CreateMenuUI()
    {
        GameObject canvasObj = new GameObject("Menu Canvas");
        canvasObj.transform.SetParent(transform);  
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;  

     
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        CreateBackground(canvasObj.transform);
        CreateTitle(canvasObj.transform);
        CreateHighScoreDisplay(canvasObj.transform);
        CreateStartButton(canvasObj.transform);
        CreateControls(canvasObj.transform);
        CreatePressStartText(canvasObj.transform);

        Debug.Log("Menu UI created successfully");
    }

    void CreateBackground(Transform parent)
    {
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(parent, false);

        Image bg = bgObj.AddComponent<Image>();
        bg.color = Color.black;
        bg.raycastTarget = true;

        RectTransform rt = bg.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    void CreateTitle(Transform parent)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent, false);  

        Text title = titleObj.AddComponent<Text>();
        title.text = "PAC";  
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 80;  
        title.fontStyle = FontStyle.Bold;
        title.color = Color.yellow;
        title.alignment = TextAnchor.MiddleCenter;
        title.horizontalOverflow = HorizontalWrapMode.Overflow;
        title.verticalOverflow = VerticalWrapMode.Overflow;

        Outline outline = titleObj.AddComponent<Outline>();
        outline.effectColor = Color.blue;
        outline.effectDistance = new Vector2(4, -4);

        RectTransform rt = title.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.7f);
        rt.anchorMax = new Vector2(0.5f, 0.7f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(800, 150);

        titleObj.AddComponent<PulseAnimation>();
    }

    void CreateHighScoreDisplay(Transform parent)
    {
        GameObject scoreObj = new GameObject("HighScore");
        scoreObj.transform.SetParent(parent, false);

        Text scoreText = scoreObj.AddComponent<Text>();
        scoreText.text = $"HIGH SCORE\n{highScore:D5}";
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        scoreText.fontSize = 28;
        scoreText.color = Color.white;
        scoreText.alignment = TextAnchor.MiddleCenter;

        RectTransform rt = scoreText.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(400, 80);
    }

    void CreateStartButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("StartButton");
        buttonObj.transform.SetParent(parent, false);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.13f, 0.13f, 1f, 0.9f);

        Button button = buttonObj.AddComponent<Button>();

        RectTransform buttonRt = buttonObj.GetComponent<RectTransform>();
        buttonRt.anchorMin = new Vector2(0.5f, 0.35f);
        buttonRt.anchorMax = new Vector2(0.5f, 0.35f);
        buttonRt.pivot = new Vector2(0.5f, 0.5f);
        buttonRt.anchoredPosition = Vector2.zero;
        buttonRt.sizeDelta = new Vector2(300, 70);

        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(buttonObj.transform, false);

        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = "START GAME";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 30;
        buttonText.color = Color.yellow;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.horizontalOverflow = HorizontalWrapMode.Overflow;
        buttonText.verticalOverflow = VerticalWrapMode.Overflow;

        Outline textOutline = textObj.AddComponent<Outline>();
        textOutline.effectColor = Color.black;
        textOutline.effectDistance = new Vector2(2, -2);

        RectTransform textRt = buttonText.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        textRt.anchoredPosition = Vector2.zero;

   
        button.onClick.AddListener(() => {
            Debug.Log("?? Button clicked!");
            StartGame();
        });

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.13f, 0.13f, 1f, 0.9f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 1f, 1f);
        colors.pressedColor = new Color(0.5f, 0.5f, 1f, 1f);
        button.colors = colors;

        Debug.Log("Start button created");
    }

    void CreateControls(Transform parent)
    {
        GameObject controlsObj = new GameObject("Controls");
        controlsObj.transform.SetParent(parent, false);

        Text controlsText = controlsObj.AddComponent<Text>();
        controlsText.text = "CONTROLS: WASD or ARROW KEYS\n\nClick START GAME to begin";
        controlsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        controlsText.fontSize = 18;
        controlsText.color = new Color(0.8f, 0.8f, 0.8f);
        controlsText.alignment = TextAnchor.MiddleCenter;

        RectTransform rt = controlsText.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.2f);
        rt.anchorMax = new Vector2(0.5f, 0.2f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(600, 80);
    }

    void CreatePressStartText(Transform parent)
    {
        GameObject textObj = new GameObject("PressStart");
        textObj.transform.SetParent(parent, false);

        Text text = textObj.AddComponent<Text>();
        text.text = "PRESS START";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 20;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        RectTransform rt = text.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.1f);
        rt.anchorMax = new Vector2(0.5f, 0.1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(400, 30);

        textObj.AddComponent<BlinkAnimation>();
    }

 
    void StartGame()
    {
        if (gameStarted) return;
        gameStarted = true;

        Debug.Log("=== STARTING GAME ===");

      
        if (canvas != null)
        {
            Destroy(canvas.gameObject);
            Debug.Log("Menu destroyed");
        }

        Destroy(gameObject);

       
        GameObject gameRoot = new GameObject("=== GAME ROOT ===");
        gameRoot.AddComponent<GameManager>();

        Debug.Log("GameManager created - game should start now!");

    
        Time.timeScale = 1f;

       
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound("PowerPellet");
        }
    }
}

public class PulseAnimation : MonoBehaviour
{
    private Text text;
    private float timer = 0f;

    void Start()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        timer += Time.deltaTime * 2f;  
        float scale = 1f + Mathf.Sin(timer) * 0.15f;
        transform.localScale = Vector3.one * scale;
    }
}

public class BlinkAnimation : MonoBehaviour
{
    private Text text;
    private float timer = 0f;

    void Start()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        timer += Time.deltaTime * 2f;  
        Color newColor = Color.white;
        newColor.a = (Mathf.Sin(timer) + 1f) * 0.5f;
        text.color = newColor;
    }
}