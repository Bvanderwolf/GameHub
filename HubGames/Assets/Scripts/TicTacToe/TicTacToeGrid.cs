using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeGrid : MonoBehaviour
{
    public event Action OnGameOver;

    [SerializeField]
    private RectTransform canvasTF;

    [SerializeField]
    private GameObject image;

    private AudioClip tapSound;
    private AudioSource audioSource;

    [SerializeField]
    private TicTacToeGameState gameState;

    private struct TakenCell
    {
        public int numOfPlayer;
        public Vector2Int position;

        public TakenCell (int nop, Vector2Int p)
        {
            numOfPlayer = nop;
            position = p;
        }
    }

    private readonly int gridSizeXY = 3;

    private Vector2[,] cells;
    private List<TakenCell> takenCells = new List<TakenCell>();

    private float cellHalfSize;

    private readonly List<Vector2Int> sequenceOrientations = new List<Vector2Int>()
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(1, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(1, -1),
        new Vector2Int (-1, -1)
};

    // Start is called before the first frame update
    private void Start ()
    {
        BuildGrid();
        SetWidthHeightOfImage();

        tapSound = ResourceManager.GetResource<AudioClip>("place");
        audioSource = GetComponent<AudioSource>();

        //attach OncanvasClicked function to OnCanvasClicked event from input system
        InputSystem.Instance.OnCanvasClicked += OnCanvasClicked;
        InputSystem.Instance.OnGameRestartInput += OnGridSystemRestart;
    }

    private void BuildGrid ()
    {
        //get grid layout group component reference and initialise cells array
        cells = new Vector2[gridSizeXY, gridSizeXY];

        cellHalfSize = canvasTF.rect.width / (gridSizeXY * 2);

        float cellStartPosX = 0;
        float cellStartPosY = 0;

        //setup cells array with cell positions
        for (int y = gridSizeXY - 1; y >= 0; y--)
        {
            cellStartPosY = (canvasTF.rect.height / gridSizeXY) * (y - 1);
            for (int x = 0; x < gridSizeXY; x++)
            {
                cellStartPosX = (canvasTF.rect.width / gridSizeXY) * (x - 1);
                cells[x, y] = new Vector2(canvasTF.anchoredPosition.x + cellStartPosX, canvasTF.anchoredPosition.y + cellStartPosY);
            }
        }
    }

    private void SetWidthHeightOfImage ()
    {
        RectTransform rectTF = image.GetComponent<RectTransform>();
        float xSize = rectTF.sizeDelta.x * HubSettings.Instance.ScreenRatio.x;
        float ySize = rectTF.sizeDelta.y * HubSettings.Instance.ScreenRatio.y;
        if (xSize > ySize) xSize = ySize;
        else if (ySize > xSize) ySize = xSize;
        rectTF.sizeDelta = new Vector2(xSize, ySize);
    }

    private void OnDestroy ()
    {
        InputSystem.Instance.OnCanvasClicked -= OnCanvasClicked;
        InputSystem.Instance.OnGameRestartInput -= OnGridSystemRestart;
    }

    //when the canvas is pressed we check in which cell and if it is not taken, we place an item
    public void OnCanvasClicked (Vector3 position)
    {
        if (TimerManager.Instance.IsTimingStartTimer() ||
            TicTacToeCanvas.PoppingUpPlayerPlayingText ||
            TicTacToeGameState.GameOver)
            return;

        for (int x = 0; x < cells.GetLength(0); x++)
        {
            for (int y = 0; y < cells.GetLength(1); y++)
            {
                if (InsideCell(position, cells[x, y]))
                {
                    /*we check for each takencell whether any takencell its position matches the
                     pressed cell its position*/
                    Vector2Int pressed_cell_position = new Vector2Int(x, y);
                    if (takenCells.Any((takencell) => takencell.position == pressed_cell_position))
                    {
                        Debug.Log("cell already taken");
                    }
                    else
                    {
                        PlaceItemOnBoard(cells[x, y], new Vector2Int(x, y));
                        CheckOnGameOverConditions();
                        gameState.SwitchTurns();
                    }
                    x = cells.GetLength(0);
                    break;
                }
            }
        }
    }

    private void OnGridSystemRestart ()
    {
        takenCells.Clear();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("TicTacItem"))
        {
            Destroy(obj);
        }
    }

    /// <summary>
    /// Checks whether the game has ender through one of 2 conditions: full board or 3 items in a row
    /// </summary>
    private void CheckOnGameOverConditions ()
    {
        //only if the cell count is higher than 3 we check for game over condition
        if (takenCells.Count >= gridSizeXY)
        {
            //the game ends if the board has been filled with items
            if (takenCells.Count == cells.Length)
            {
                gameState.SetNoWinner();
                OnGameOver?.Invoke();
            }
            else
            {
                /*for each taken cell we traverse all orientations, taking into account the player number,
                 looking for a sequence of 3 items in a row*/
                foreach (TakenCell takencell in takenCells)
                {
                    foreach (Vector2Int sequenceOrientation in sequenceOrientations)
                    {
                        getNextCellInSequence(
                            takencell,
                            new TakenCell(takencell.numOfPlayer, takencell.position + sequenceOrientation),
                            sequenceOrientation);
                    }
                }
            }
        }
    }

    /// <summary>
    /// traverses to next cell based on starting cell and orientation
    /// next cell parameter is used to recursively repeat function when a sequence is found
    /// </summary>
    /// <param name="startCell"></param>
    /// <param name="nextCell"></param>
    /// <param name="orientation"></param>
    private void getNextCellInSequence (TakenCell startCell, TakenCell nextCell, Vector2Int orientation)
    {
        int diffX = Mathf.Abs(startCell.position.x - nextCell.position.x);
        int diffY = Mathf.Abs(startCell.position.y - nextCell.position.y);

        if (takenCells.Contains(nextCell))
        {
            getNextCellInSequence(startCell, new TakenCell(startCell.numOfPlayer, nextCell.position + orientation), orientation);
        }

        if (diffX == gridSizeXY || diffY == gridSizeXY)
        {
            if (!TicTacToeGameState.GameOver)
            {
                OnGameOver?.Invoke();
                Debug.Log("full sequence found");
            }
        }
    }

    /// <summary>
    /// Places item with right player sprite based on turn on given position
    /// </summary>
    /// <param name="position">position of cell in world space</param>
    /// <param name="cell">position of cell in cells array</param>
    private void PlaceItemOnBoard (Vector2 position, Vector2Int cell)
    {
        Debug.Log("places item on cell with position " + position + "on cell" + cell);
        Instantiate(image, position, Quaternion.identity, this.transform)
            .GetComponent<Image>().sprite = gameState.GetPlayerSprite();

        takenCells.Add(new TakenCell(TicTacToeGameState.NumOfPlayerPlaying, cell));
        audioSource?.PlayOneShot(tapSound);
    }

    /// <summary>
    /// returns whether a position is inside given cell or not
    /// </summary>
    /// <param name="touchPos"></param>
    /// <param name="cellPos"></param>
    /// <returns></returns>
    private bool InsideCell (Vector2 touchPos, Vector2 cellPos)
    {
        return Vector3.Distance(touchPos, cellPos) < cellHalfSize;
    }
}