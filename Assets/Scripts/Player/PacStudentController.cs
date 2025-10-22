using UnityEngine;


public class PacStudentController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;

    [Header("Debug Info")]
    [SerializeField] private string debugPlayerInput = "null";
    [SerializeField] private string debugLastInput = "null";
    [SerializeField] private string debugCurrentInput = "null";
    [SerializeField] private float debugLerpProgress = 0f;

   
    private string playerInput = null;    
    private string lastInput = null;      
    private string currentInput = null;   

 
    private Vector2Int currentGridPos;
    private Vector2Int targetGridPos;
    private float lerpProgress = 1f;      

  
    private SpriteRenderer spriteRenderer;
    private GridManager grid;

 
    private float animTimer = 0f;
    private bool mouthOpen = true;

    void Start()
    {
       
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer not found on PacStudent. Animation effects disabled.");
        }

        grid = GridManager.Instance;
    

        
        currentGridPos = LevelData.GetPlayerStartPosition();
        targetGridPos = currentGridPos;
        transform.position = grid.GridToWorld(currentGridPos);

        Debug.Log($"PacStudent initialized at grid position: {currentGridPos}");
    }

    void Update()
    {
        
        ReadInput();

        
        if (lerpProgress >= 1f)
        {
            DecideNextMove();
        }

        SmoothMove();
        UpdateVisuals();
        UpdateDebugInfo();
    }

    public void ResetPosition()
    {
        currentGridPos = LevelData.GetPlayerStartPosition();
        targetGridPos = currentGridPos;
        transform.position = grid.GridToWorld(currentGridPos);

        playerInput = null;
        lastInput = null;
        currentInput = null;
        lerpProgress = 1f;

        Debug.Log("Player position reset");
    }


    void ReadInput()
    {
        playerInput = null; 

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            playerInput = "w";
            lastInput = "w";
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            playerInput = "s";
            lastInput = "s";
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            playerInput = "a";
            lastInput = "a";
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            playerInput = "d";
            lastInput = "d";
        }
    }


    void DecideNextMove()
    {
  
        currentGridPos = targetGridPos;
        CheckPelletCollection();


        if (lastInput != null && CanMove(lastInput))
        {
            currentInput = lastInput;
            StartNewMove(currentInput);
            return;
        }


        if (currentInput != null && CanMove(currentInput))
        {
            StartNewMove(currentInput);
            return;
        }

        currentInput = null;
        lerpProgress = 1f;

        Debug.Log("PacStudent stopped - no valid move");
    }


    bool CanMove(string direction)
    {
        Vector2Int nextPos = currentGridPos + GetDirectionVector(direction);
        return grid.IsWalkable(nextPos);
    }

 
    void StartNewMove(string direction)
    {
        targetGridPos = currentGridPos + GetDirectionVector(direction);
        lerpProgress = 0f;

      
    }


    void SmoothMove()
    {
        if (currentInput == null || lerpProgress >= 1f)
        {
            return;
        }

   
        lerpProgress += moveSpeed * Time.deltaTime;
        lerpProgress = Mathf.Clamp01(lerpProgress);


        Vector3 startPos = grid.GridToWorld(currentGridPos);
        Vector3 endPos = grid.GridToWorld(targetGridPos);
        transform.position = Vector3.Lerp(startPos, endPos, lerpProgress);
    }


    void UpdateVisuals()
    {
        if (currentInput != null)
        {
            float angle = GetDirectionAngle(currentInput);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (spriteRenderer == null)
        {
            return;
        }

        animTimer += Time.deltaTime * 10f;
        if (animTimer >= 1f)
        {
            animTimer = 0f;
            mouthOpen = !mouthOpen;

            float brightness = mouthOpen ? 1f : 0.8f;
            spriteRenderer.color = new Color(1f, brightness, 0f);
        }
    }


    void CheckPelletCollection()
    {
        int gridType = grid.GetGridType(currentGridPos);

        if (gridType == GridManager.PELLET)
        {
            grid.SetGridType(currentGridPos, GridManager.EMPTY);
            GameManager.Instance.AddScore(10);
            GameManager.Instance.RemovePellet(currentGridPos);
            AudioManager.Instance?.PlaySound("PelletEat");
            Debug.Log("Pellet collected! +10");
        }
        else if (gridType == GridManager.POWER_PELLET)
        {
            grid.SetGridType(currentGridPos, GridManager.EMPTY);
            GameManager.Instance.AddScore(50);
            GameManager.Instance.ActivatePowerMode();
            GameManager.Instance.RemovePellet(currentGridPos);
            AudioManager.Instance?.PlaySound("PowerPellet");
            Debug.Log("Power Pellet collected! +50 - POWER MODE!");
        }
    }


    Vector2Int GetDirectionVector(string direction)
    {
        switch (direction)
        {
            case "w": return Vector2Int.up;
            case "s": return Vector2Int.down;
            case "a": return Vector2Int.left;
            case "d": return Vector2Int.right;
            default: return Vector2Int.zero;
        }
    }


    float GetDirectionAngle(string direction)
    {
        switch (direction)
        {
            case "d": return 0f;
            case "w": return 90f;
            case "a": return 180f;
            case "s": return 270f;
            default: return 0f;
        }
    }


    void UpdateDebugInfo()
    {
        debugPlayerInput = playerInput ?? "null";
        debugLastInput = lastInput ?? "null";
        debugCurrentInput = currentInput ?? "null";
        debugLerpProgress = lerpProgress;
    }

    public Vector2Int GetGridPosition()
    {
        return currentGridPos;
    }

    public Vector2Int GetCurrentDirection()
    {
        if (currentInput == null)
        {
            return Vector2Int.zero;
        }
        return GetDirectionVector(currentInput);
    }
}
