using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class SnakeGrid : MonoBehaviour
{
    [SerializeField, Tooltip("total gridsize meaning x times y")]
    private int gridSize;

    [SerializeField] private RectTransform canvasTF;
    [SerializeField] private GameObject snakeTarget;

    [SerializeField] private GameObject snakeHead;

    private Vector2[,] gridPositions;

    public static readonly int SPAWNMARGIN = 6;

    public static event Action OnGridCollision;

    private GameObject currentSnakeTarget = null;

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

    // Start is called before the first frame update
    private void Start ()
    {
        BuildGrid();
        SetWidthHeightOfPart(snakeTarget);
        SnakeUI.OnCountDownEnd += OnStartSnakeGame;
    }

    private void BuildGrid ()
    {
        int gridXY = Mathf.RoundToInt((float)gridSize / 2);
        gridPositions = new Vector2[gridXY, gridXY];

        float cellSize = canvasTF.rect.height / (float)gridXY;

        float halfSize = cellSize / 2;

        for (int x = 0; x < gridPositions.GetLength(0); x++)
        {
            for (int y = 0; y < gridPositions.GetLength(1); y++)
            {
                gridPositions[x, y] = new Vector2(halfSize + x * cellSize, halfSize + y * cellSize);
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

    private void OnDestroy ()
    {
        SnakeUI.OnCountDownEnd -= OnStartSnakeGame;
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
        currentSnakeTarget = Instantiate(snakeTarget, gridPositions[gridX, gridY], Quaternion.identity, transform);
    }

    /// <summary>
    /// returns a vector2 position inside the grid. You give the gridpositon as reference to let the function
    /// modify its value when the gridposition is out of bounds
    /// </summary>
    /// <param name="gridPosition"></param>
    /// <returns></returns>
    public Vector2 GetGridPosition (ref Vector2Int gridPosition)
    {
        if (gridPosition.x == gridPositions.GetLength(0)) gridPosition.x = 0;
        if (gridPosition.x < 0) gridPosition.x = gridPositions.GetLength(0) - 1;
        if (gridPosition.y == gridPositions.GetLength(1)) gridPosition.y = 0;
        if (gridPosition.y < 0) gridPosition.y = gridPositions.GetLength(1) - 1;

        return gridPositions[gridPosition.x, gridPosition.y];
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
        int randomGridPointX = Random.Range(SPAWNMARGIN, gridPositions.GetLength(0) - SPAWNMARGIN);
        int randomGridPointY = Random.Range(SPAWNMARGIN, gridPositions.GetLength(1) - SPAWNMARGIN);
        GameObject snake = Instantiate(snakeHead,
            (Vector3)gridPositions[randomGridPointX, randomGridPointY],
            Quaternion.identity,
            transform);

        SnakeController controller = snake.GetComponent<SnakeController>();
        //the start part count of the snake can't be larger than the spawnMargin
        if (controller.StartPartCount > SPAWNMARGIN)
        {
            controller.StartPartCount = SPAWNMARGIN;
            Debug.LogWarning("start part count is higher than spawn margin, chance of out of bounds error :: Changing it to be equal");
        }

        if (controller)
        {
            controller.Init(this, gridPositions, new Vector2Int(randomGridPointX, randomGridPointY));
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