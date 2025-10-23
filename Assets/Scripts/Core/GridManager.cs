using UnityEngine;


public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

 
    public float gridSize = 1f;
    public int gridWidth = 28;
    public int gridHeight = 31;

   
    private int[,] gridData;


    public const int EMPTY = 0;
    public const int WALL = 1;
    public const int PELLET = 2;
    public const int POWER_PELLET = 3;
    public const int GHOST_HOUSE = 4;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void InitializeGrid(int[,] levelData)
    {
        gridData = levelData;
        gridWidth = levelData.GetLength(1);
        gridHeight = levelData.GetLength(0);

        Debug.Log($"Grid initialized: {gridWidth}x{gridHeight}");
    }


    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * gridSize,
            gridPos.y * gridSize,
            0
        );
    }


    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / gridSize),
            Mathf.RoundToInt(worldPos.y / gridSize)
        );
    }


    public bool IsWalkable(Vector2Int gridPos)
    {
      
        if (gridPos.x < 0 || gridPos.x >= gridWidth ||
            gridPos.y < 0 || gridPos.y >= gridHeight)
        {
            return false;
        }

    
        int cellType = gridData[gridPos.y, gridPos.x];
        return cellType != WALL;
    }


    public int GetGridType(Vector2Int gridPos)
    {
        if (gridPos.x < 0 || gridPos.x >= gridWidth ||
            gridPos.y < 0 || gridPos.y >= gridHeight)
        {
            return WALL;
        }

        return gridData[gridPos.y, gridPos.x];
    }


    public void SetGridType(Vector2Int gridPos, int type)
    {
        if (gridPos.x >= 0 && gridPos.x < gridWidth &&
            gridPos.y >= 0 && gridPos.y < gridHeight)
        {
            gridData[gridPos.y, gridPos.x] = type;
        }
    }
}
