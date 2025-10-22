using UnityEngine;

public enum GhostType
{
    Blinky, 
    Pinky,   
    Inky,    
    Clyde    
}

public enum GhostMode
{
    Chase,      
    Scatter,    
    Frightened, 
    Dead       
}


public class GhostController : MonoBehaviour
{
    [Header("Settings")]
    public GhostType ghostType;
    public float moveSpeed = 3f;
    public float frightenedSpeed = 2f;
    public float deadSpeed = 6f;

    [Header("Current State")]
    public GhostMode currentMode = GhostMode.Scatter;

    [Header("Debug")]
    [SerializeField] private Vector2Int debugCurrentPos;
    [SerializeField] private Vector2Int debugTargetPos;
    [SerializeField] private Vector2Int debugNextMove;

    private Vector2Int currentGridPos;
    private Vector2Int targetGridPos;
    private Vector2Int homePosition;
    private float lerpProgress = 1f;

    private GridManager grid;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private AStar pathfinder;


    private float modeTimer = 0f;
    private const float SCATTER_DURATION = 7f;
    private const float CHASE_DURATION = 20f;


    private bool isBlinking = false;
    private float blinkTimer = 0f;

    void Start()
    {
        grid = GridManager.Instance;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"{ghostType} missing SpriteRenderer, disabling ghost.");
            enabled = false;
            return;
        }
        originalColor = spriteRenderer.color;

        currentGridPos = grid.WorldToGrid(transform.position);
        targetGridPos = currentGridPos;
        homePosition = GetHomePosition();

 
        pathfinder = new AStar(grid);

        Debug.Log($"{ghostType} ghost initialized at {currentGridPos}, home: {homePosition}");
    }

    void Update()
    {
        UpdateMode();

        if (lerpProgress >= 1f)
        {
            DecideNextMove();
        }

        SmoothMove();
        UpdateVisuals();
        UpdateDebugInfo();
    }


    void UpdateMode()
    {
        if (currentMode == GhostMode.Frightened || currentMode == GhostMode.Dead)
        {
            return;
        }

        modeTimer += Time.deltaTime;


        if (currentMode == GhostMode.Scatter && modeTimer >= SCATTER_DURATION)
        {
            currentMode = GhostMode.Chase;
            modeTimer = 0f;
            Debug.Log($"{ghostType} switched to CHASE mode");
        }
        else if (currentMode == GhostMode.Chase && modeTimer >= CHASE_DURATION)
        {
            currentMode = GhostMode.Scatter;
            modeTimer = 0f;
            Debug.Log($"{ghostType} switched to SCATTER mode");
        }
    }


    void DecideNextMove()
    {
        currentGridPos = targetGridPos;

        Vector2Int targetPosition = GetTargetPosition();
        Vector2Int nextDirection = Vector2Int.zero;

        if (currentMode == GhostMode.Frightened)
        {
 
            nextDirection = GetRandomDirection();
        }
        else
        {
       
            nextDirection = pathfinder.FindNextDirection(currentGridPos, targetPosition);
        }

        debugNextMove = nextDirection;

        if (nextDirection != Vector2Int.zero && grid.IsWalkable(currentGridPos + nextDirection))
        {
            targetGridPos = currentGridPos + nextDirection;
            lerpProgress = 0f;
        }
    }


    Vector2Int GetTargetPosition()
    {
        debugTargetPos = Vector2Int.zero;

    
        if (currentMode == GhostMode.Dead)
        {
            debugTargetPos = homePosition;
            return homePosition;
        }

     
        if (currentMode == GhostMode.Scatter)
        {
            Vector2Int corner = GetScatterCorner();
            debugTargetPos = corner;
            return corner;
        }

     
        Vector2Int playerPos = GameManager.Instance.GetPlayerGridPosition();
        Vector2Int target = playerPos;

        switch (ghostType)
        {
            case GhostType.Blinky:
             
                target = playerPos;
                break;

            case GhostType.Pinky:

                Vector2Int pinkyDirection = GameManager.Instance.GetPlayerDirection();
                target = playerPos + pinkyDirection * 4;
                break;

            case GhostType.Inky:

                GameObject blinky = GameObject.Find("Blinky");
                if (blinky != null)
                {
                    Vector2Int blinkyPos = grid.WorldToGrid(blinky.transform.position);
                    Vector2Int inkyDirection = GameManager.Instance.GetPlayerDirection();
                    Vector2Int offset = playerPos + inkyDirection * 2 - blinkyPos;
                    target = blinkyPos + offset * 2;
                }
                else
                {
                    target = playerPos;
                }
                break;

            case GhostType.Clyde:
               
                float distance = Vector2Int.Distance(currentGridPos, playerPos);
                if (distance > 8)
                {
                    target = playerPos;
                }
                else
                {
                    target = GetScatterCorner();
                }
                break;
        }

        debugTargetPos = target;
        return target;
    }


    Vector2Int GetScatterCorner()
    {
        switch (ghostType)
        {
            case GhostType.Blinky: return new Vector2Int(18, 0);
            case GhostType.Pinky: return new Vector2Int(0, 0);
            case GhostType.Inky: return new Vector2Int(18, 20);
            case GhostType.Clyde: return new Vector2Int(0, 20);
            default: return Vector2Int.zero;
        }
    }


    Vector2Int GetHomePosition()
    {
        return new Vector2Int(9, 9); 
    }


    Vector2Int GetRandomDirection()
    {
        Vector2Int[] directions = {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

   
        for (int i = 0; i < directions.Length; i++)
        {
            int randomIndex = Random.Range(i, directions.Length);
            Vector2Int temp = directions[i];
            directions[i] = directions[randomIndex];
            directions[randomIndex] = temp;
        }


        foreach (Vector2Int dir in directions)
        {
            if (grid.IsWalkable(currentGridPos + dir))
            {
                return dir;
            }
        }

        return Vector2Int.zero;
    }


    void SmoothMove()
    {
        if (lerpProgress >= 1f) return;

        float speed = moveSpeed;
        if (currentMode == GhostMode.Frightened) speed = frightenedSpeed;
        else if (currentMode == GhostMode.Dead) speed = deadSpeed;

        lerpProgress += speed * Time.deltaTime;
        lerpProgress = Mathf.Clamp01(lerpProgress);

        Vector3 startPos = grid.GridToWorld(currentGridPos);
        Vector3 endPos = grid.GridToWorld(targetGridPos);
        transform.position = Vector3.Lerp(startPos, endPos, lerpProgress);
    }


    void UpdateVisuals()
    {
        if (currentMode == GhostMode.Frightened)
        {
        
            float timeLeft = GameManager.Instance.powerModeTimer;

            if (timeLeft < 3f)
            {
              
                blinkTimer += Time.deltaTime * 8f;
                if (blinkTimer >= 1f)
                {
                    blinkTimer = 0f;
                    isBlinking = !isBlinking;
                }

                spriteRenderer.color = isBlinking ? Color.white : Color.blue;
            }
            else
            {
                spriteRenderer.color = Color.blue;
            }
        }
        else if (currentMode == GhostMode.Dead)
        {
         
            Color c = originalColor;
            c.a = 0.3f;
            spriteRenderer.color = c;
        }
        else
        {
           
            spriteRenderer.color = originalColor;
        }
    }


    public void EnterFrightenedMode()
    {
        if (currentMode != GhostMode.Dead)
        {
            currentMode = GhostMode.Frightened;
            Debug.Log($"{ghostType} entered FRIGHTENED mode");
        }
    }


    public void ExitFrightenedMode()
    {
        if (currentMode == GhostMode.Frightened)
        {
            currentMode = GhostMode.Chase;
            isBlinking = false;
            Debug.Log($"{ghostType} exited FRIGHTENED mode");
        }
    }


    public void Die()
    {
        currentMode = GhostMode.Dead;
        Debug.Log($"{ghostType} was eaten! Returning to base...");
        AudioManager.Instance?.PlaySound("GhostEaten");
    }


    public void Respawn()
    {
        if (currentMode == GhostMode.Dead && currentGridPos == homePosition)
        {
            currentMode = GhostMode.Scatter;
            modeTimer = 0f;
            Debug.Log($"{ghostType} respawned!");
        }
    }


    void CheckRespawn()
    {
        if (currentMode == GhostMode.Dead && lerpProgress >= 1f)
        {
            if (currentGridPos == homePosition)
            {
                Respawn();
            }
        }
    }

 
    public Vector2Int GetGridPosition()
    {
        return currentGridPos;
    }

   
    public bool IsVulnerable()
    {
        return currentMode == GhostMode.Frightened;
    }

 
    void UpdateDebugInfo()
    {
        debugCurrentPos = currentGridPos;
        CheckRespawn();
    }
}
