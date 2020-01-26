using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class SnakeGrid : MonoBehaviour
{
    [SerializeField] private RectTransform canvasTF;
    [SerializeField] private GameObject snakeTarget;

    [SerializeField] private GameObject snakeHead;

    public Vector2[,] gridPositions { get; private set; }

    public event Action OnGridCollision;

    private GameObject currentSnakeTarget = null;
    private readonly int pixelsPerGridCell = 20;

    public Vector3 SnakeTargetPosition
    {
        get
        {
            if (currentSnakeTarget)
            {
                return currentSnakeTarget.transform.position;
            }
            else
            {
                return Vector3.zero;
            }
        }
    }

    public Vector2Int SnakeTargetGridPosition { get; private set; }

    // Start is called before the first frame update
    private void Start ()
    {
        BuildGrid();
        SetWidthHeightOfPart(snakeTarget);
        FindObjectOfType<SnakeUI>().OnCountDownEnd += OnStartSnakeGame;
    }

    private void BuildGrid ()
    {
        float gridSizeX = (float)Screen.width / pixelsPerGridCell;
        float gridSizeY = (float)Screen.height / pixelsPerGridCell;
        int gridX = Mathf.RoundToInt(gridSizeX * 0.5f);
        int gridY = Mathf.RoundToInt(gridSizeY * 0.5f);

        gridPositions = new Vector2[gridX, gridY];

        float cellSize = canvasTF.rect.height / ((gridX + gridY) * 0.5f);
        float cellSizeX = Screen.width / gridX;
        float cellSizeY = Screen.height / gridY;

        float halfSize = cellSize / 2;

        for (int x = 0; x < gridPositions.GetLength(0); x++)
        {
            for (int y = 0; y < gridPositions.GetLength(1); y++)
            {
                gridPositions[x, y] = new Vector2(halfSize + x * cellSizeX, halfSize + y * cellSizeY);
            }
        }
    }

    public void SetWidthHeightOfPart (GameObject part)
    {
        RectTransform rectTF = part.GetComponent<RectTransform>();
        float xSize = rectTF.sizeDelta.x * HubSettings.Instance.ScreenRatio.x;
        float ySize = rectTF.sizeDelta.y * HubSettings.Instance.ScreenRatio.y;
        if (xSize > ySize) xSize = ySize;
        else if (ySize > xSize) ySize = xSize;
        rectTF.sizeDelta = new Vector2(xSize, ySize);
    }

    private void OnSnakeSelfCollision ()
    {
        //when the snake collides with itself, the snaketarget is destroyed and an event is raised
        if (currentSnakeTarget)
        {
            Destroy(currentSnakeTarget);
        }
        OnGridCollision?.Invoke();
    }

    private void OnSnakeTargetCollision ()
    {
        //when the snake collides with the target, the target is destroyed and a new one is spawned
        if (currentSnakeTarget)
        {
            Destroy(currentSnakeTarget);
        }
        SpawnSnakeTarget();
    }

    /// <summary>
    /// spawns a snake target at a random location inside the grid bounds
    /// </summary>
    private void SpawnSnakeTarget ()
    {
        int gridX = Random.Range(0, gridPositions.GetLength(0));
        int gridY = Random.Range(0, gridPositions.GetLength(1));
        SnakeTargetGridPosition = new Vector2Int(gridX, gridY);
        currentSnakeTarget = Instantiate(snakeTarget, gridPositions[gridX, gridY], Quaternion.identity, transform);
    }

    /// <summary>
    /// returns a vector2 position inside the grid. You give the gridpositon as reference to let the function
    /// modify its value when the gridposition is out of bounds
    /// </summary>
    /// <param name="gridPosition"></param>
    /// <returns></returns>
    public Vector2 GetGridPosition (Vector2Int gridPosition)
    {
        int x = gridPosition.x;
        int y = gridPosition.y;
        if (x == gridPositions.GetLength(0)) x = 0;
        if (x < 0) x = gridPositions.GetLength(0) - 1;
        if (y == gridPositions.GetLength(1)) y = 0;
        if (y < 0) y = gridPositions.GetLength(1) - 1;
        return gridPositions[x, y];
    }

    /// <summary>
    /// returns a vector2Int inside the bounds of the grid based on given vector2 positon
    /// returns vector2int.zero if position is not found in grid array
    /// </summary>
    /// <param name="gridPosition"></param>
    /// <returns></returns>
    public Vector2Int GetGridPosition (Vector2 gridPosition)
    {
        for (int x = 0; x < gridPositions.GetLength(0); x++)
        {
            for (int y = 0; y < gridPositions.GetLength(1); y++)
            {
                if (gridPositions[x, y] == gridPosition)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return new Vector2Int();
    }

    //when starting the snake game the snake is spawned on a random position inside the grid and initialized
    private void OnStartSnakeGame ()
    {
        GameObject snake = Instantiate(snakeHead, transform);
        SnakeController controller = snake.GetComponent<SnakeController>();

        int randomGridPointX = Random.Range(controller.spawnMargin, gridPositions.GetLength(0) - controller.spawnMargin);
        int randomGridPointY = Random.Range(controller.spawnMargin, gridPositions.GetLength(1) - controller.spawnMargin);

        //the start part count of the snake can't be larger than the spawnMargin
        if (controller.StartPartCount > controller.spawnMargin)
        {
            controller.StartPartCount = controller.spawnMargin;
            Debug.LogWarning("start part count is higher than spawn margin, chance of out of bounds error :: Changing it to be equal");
        }

        if (controller)
        {
            controller.Init(this, new Vector2Int(randomGridPointX, randomGridPointY));
            controller.OnSelfCollision += OnSnakeSelfCollision;
            controller.OnTargetCollision += OnSnakeTargetCollision;
        }
        else
        {
            Debug.LogError("could not build snake :: controller is null");
        }

        SpawnSnakeTarget();
    }
}