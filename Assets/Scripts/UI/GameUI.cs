using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    private Canvas canvas;
    private Text scoreText;
    private Text timerText;
    private Text ghostTimerText;
    private Text levelNameText;
    private Transform livesContainer;
    private readonly List<Image> lifeIcons = new List<Image>();
    private Sprite lifeSprite;

    private const string SCORE_PREFIX = "SCORE: ";
    private const string TIMER_DEFAULT = "00:00:00";

    void Start()
    {
        lifeSprite = SpriteFactory.CreateCircleSprite(Color.white, 64);
        CreateUI();
        RefreshAll();
    }

    void Update()
    {
        RefreshAll();
    }

    void CreateUI()
    {
        GameObject canvasObj = new GameObject("UI Canvas");
        canvasObj.transform.SetParent(transform);

        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        CreateScoreDisplay(canvasObj.transform);
        CreateLivesDisplay(canvasObj.transform);
        CreateTimerDisplay(canvasObj.transform);
        CreateGhostTimerDisplay(canvasObj.transform);
        CreateLevelNameDisplay(canvasObj.transform);
    }

    void CreateScoreDisplay(Transform parent)
    {
        GameObject scoreObj = new GameObject("ScoreText");
        scoreObj.transform.SetParent(parent);
        scoreText = SetupText(scoreObj, new Vector2(20, -20), new Vector2(0f, 1f), TextAnchor.UpperLeft, 32);
        scoreText.text = SCORE_PREFIX + "000000";
    }

    void CreateLivesDisplay(Transform parent)
    {
        GameObject container = new GameObject("LivesContainer");
        container.transform.SetParent(parent);
        livesContainer = container.transform;

        RectTransform rt = container.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(20f, -70f);
        rt.sizeDelta = new Vector2(300f, 60f);

        HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlHeight = false;
        layout.childControlWidth = false;

        int initialLives = Mathf.Max(GameManager.Instance != null ? GameManager.Instance.lives : 3, 3);
        for (int i = 0; i < initialLives; i++)
        {
            lifeIcons.Add(CreateLifeIcon(livesContainer));
        }
    }

    Image CreateLifeIcon(Transform parent)
    {
        GameObject iconObj = new GameObject("LifeIcon");
        iconObj.transform.SetParent(parent);
        Image image = iconObj.AddComponent<Image>();
        image.sprite = lifeSprite;
        image.color = new Color(1f, 0.9f, 0.15f);

        RectTransform rt = image.rectTransform;
        rt.sizeDelta = new Vector2(40f, 40f);

        return image;
    }

    void CreateTimerDisplay(Transform parent)
    {
        GameObject timerObj = new GameObject("GameTimerText");
        timerObj.transform.SetParent(parent);
        timerText = SetupText(timerObj, new Vector2(0f, -20f), new Vector2(0.5f, 1f), TextAnchor.UpperCenter, 32);
        timerText.text = TIMER_DEFAULT;
    }

    void CreateGhostTimerDisplay(Transform parent)
    {
        GameObject ghostTimerObj = new GameObject("GhostTimerText");
        ghostTimerObj.transform.SetParent(parent);
        ghostTimerText = SetupText(ghostTimerObj, new Vector2(0f, -60f), new Vector2(0.5f, 1f), TextAnchor.UpperCenter, 28);
        ghostTimerText.gameObject.SetActive(false);
    }

    void CreateLevelNameDisplay(Transform parent)
    {
        GameObject levelNameObj = new GameObject("LevelNameText");
        levelNameObj.transform.SetParent(parent);
        levelNameText = SetupText(levelNameObj, new Vector2(-20f, -20f), new Vector2(1f, 1f), TextAnchor.UpperRight, 30);
        levelNameText.text = "LEVEL 1";
    }

    Text SetupText(GameObject obj, Vector2 anchoredPosition, Vector2 anchor, TextAnchor anchorAlignment, int fontSize)
    {
        Text text = obj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = anchorAlignment;
        text.color = Color.white;

        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2f, -2f);

        RectTransform rt = text.rectTransform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = new Vector2(400f, 80f);

        return text;
    }

    void RefreshAll()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null)
        {
            return;
        }

        UpdateScore(gm.score);
        UpdateLives(gm.lives);
        UpdateTimer(gm.GameTimerSeconds);
        UpdateGhostTimer(gm.isPowerMode ? gm.powerModeTimer : 0f, gm.isPowerMode);
        UpdateLevelName(gm.CurrentLevelName);
    }

    void UpdateScore(int score)
    {
        scoreText.text = SCORE_PREFIX + score.ToString("D6");
    }

    void UpdateLives(int lives)
    {
        while (lifeIcons.Count < lives)
        {
            lifeIcons.Add(CreateLifeIcon(livesContainer));
        }

        for (int i = 0; i < lifeIcons.Count; i++)
        {
            lifeIcons[i].enabled = i < lives;
        }
    }

    void UpdateTimer(float seconds)
    {
        timerText.text = FormatTimer(seconds);
    }

    void UpdateGhostTimer(float seconds, bool isVisible)
    {
        ghostTimerText.gameObject.SetActive(isVisible);
        if (!isVisible)
        {
            return;
        }

        ghostTimerText.text = $"POWER: {Mathf.CeilToInt(seconds)}s";
    }

    void UpdateLevelName(string levelName)
    {
        levelNameText.text = levelName.ToUpperInvariant();
    }

    string FormatTimer(float seconds)
    {
        int wholeSeconds = Mathf.FloorToInt(seconds);
        int minutes = wholeSeconds / 60;
        int secs = wholeSeconds % 60;
        int centiseconds = Mathf.FloorToInt((seconds - wholeSeconds) * 100f);
        return $"{minutes:00}:{secs:00}:{centiseconds:00}";
    }
}
