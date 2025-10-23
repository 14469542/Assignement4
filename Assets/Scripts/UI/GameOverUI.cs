using UnityEngine;
using UnityEngine.UI;




public class GameOverUI : MonoBehaviour
{
    void Start()
    {
        CreateGameOverUI();
        
        
        int currentScore = GameManager.Instance.score;
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        
        if (currentScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
            PlayerPrefs.Save();
            Debug.Log($"New high score: {currentScore}");
        }
    }
    
    void CreateGameOverUI()
    {
        
        GameObject canvasObj = new GameObject("GameOver Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200; 
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        
        CreateBackground(canvasObj.transform);
        
        
        CreateGameOverText(canvasObj.transform);
        
        
        CreateFinalScore(canvasObj.transform);
        
        
        CreateRestartButton(canvasObj.transform);
        
        
        CreateHintText(canvasObj.transform);
        
        Debug.Log("Game Over UI created");
    }
    
    void CreateBackground(Transform parent)
    {
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(parent, false);
        
        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.85f); 
        bg.raycastTarget = true; 
        
        RectTransform rt = bg.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
    
    void CreateGameOverText(Transform parent)
    {
        GameObject titleObj = new GameObject("GameOverTitle");
        titleObj.transform.SetParent(parent, false);
        
        Text title = titleObj.AddComponent<Text>();
        title.text = "GAME OVER";
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 70;
        title.fontStyle = FontStyle.Bold;
        title.color = Color.red;
        title.alignment = TextAnchor.MiddleCenter;
        title.horizontalOverflow = HorizontalWrapMode.Overflow;
        title.verticalOverflow = VerticalWrapMode.Overflow;
        
        
        Outline outline = titleObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(4, -4);
        
        RectTransform rt = title.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.65f);
        rt.anchorMax = new Vector2(0.5f, 0.65f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(1000, 120);
        
        Debug.Log("Game Over title created");
    }
    
    void CreateFinalScore(Transform parent)
    {
        GameObject scoreObj = new GameObject("FinalScore");
        scoreObj.transform.SetParent(parent, false);
        
        Text scoreText = scoreObj.AddComponent<Text>();
        scoreText.text = $"FINAL SCORE: {GameManager.Instance.score}";
        scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        scoreText.fontSize = 36;
        scoreText.color = Color.white;
        scoreText.alignment = TextAnchor.MiddleCenter;
        
        
        Outline outline = scoreObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);
        
        RectTransform rt = scoreText.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(600, 60);
    }
    
    void CreateRestartButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("RestartButton");
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
        buttonText.text = "RESTART";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 32;
        buttonText.color = Color.yellow;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.horizontalOverflow = HorizontalWrapMode.Overflow;
        
   
        Outline textOutline = textObj.AddComponent<Outline>();
        textOutline.effectColor = Color.black;
        textOutline.effectDistance = new Vector2(2, -2);
        
        RectTransform textRt = buttonText.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        textRt.anchoredPosition = Vector2.zero;
        
      
        button.onClick.AddListener(() => {
            Debug.Log("Restart button clicked");
            RestartGame();
        });
        
       
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.13f, 0.13f, 1f, 0.9f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 1f, 1f);
        colors.pressedColor = new Color(0.5f, 0.5f, 1f, 1f);
        button.colors = colors;
        
        Debug.Log("Restart button created");
    }
    
    void CreateHintText(Transform parent)
    {
        GameObject hintObj = new GameObject("Hint");
        hintObj.transform.SetParent(parent, false);
        
        Text hint = hintObj.AddComponent<Text>();
        hint.text = "Press R to restart";
        hint.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hint.fontSize = 24;
        hint.color = new Color(0.8f, 0.8f, 0.8f);
        hint.alignment = TextAnchor.MiddleCenter;
        
        RectTransform rt = hint.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.2f);
        rt.anchorMax = new Vector2(0.5f, 0.2f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(400, 40);
    }
    
    void Update()
    {
  
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R key pressed in Game Over");
            RestartGame();
        }
    }
    
    void RestartGame()
    {
        Debug.Log("🔵 ========== GameOverUI.RestartGame CALLED ==========");
    
    
    if (GameManager.Instance == null)
    {
        Debug.LogError("GameManager.Instance is NULL!");
        return;
    }
    
    Debug.Log("GameManager.Instance exists");
    
    
    Transform canvasTransform = transform.parent;
    if (canvasTransform != null)
    {
        Debug.Log($"🟡 Destroying canvas: {canvasTransform.name}");
        Destroy(canvasTransform.gameObject);
    }
    else
    {
        Debug.Log("🟡 No parent canvas, destroying self");
        Destroy(gameObject);
    }
    
    Debug.Log("🟢 Calling GameManager.RestartGame()...");
    
    
    GameManager.Instance.RestartGame();
    }
}