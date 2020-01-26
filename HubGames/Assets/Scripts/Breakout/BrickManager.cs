using UnityEngine;

public class BrickManager : MonoBehaviour
{
    [SerializeField] private GameObject brickPrefab;

    [SerializeField] private int rows;
    [SerializeField] private int collums;

    private GameObject[,] brickArray;

    private readonly float brickMargin = 0.05f;
    private readonly int maxCollums = 5;
    private readonly int defaultRows = 2;
    private readonly int defaultBrickCount = 10;

    private void Start ()
    {
        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.OnGameRestartInput += OnRestart;
        }
        FindObjectOfType<BreakoutGameState>().OnGameOver += OnGameOver;
    }

    private void OnDestroy ()
    {
        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.OnGameRestartInput -= OnRestart;
        }
    }

    private void OnRestart ()
    {
        /*if rows times collums is not equal to brick count
        we set them to default values*/
        if (collums > maxCollums)
        {
            rows = defaultRows;
            collums = maxCollums;
        }

        brickArray = new GameObject[collums, rows];

        int rowsPerColor = 0;

        /*base the ammount of rows per color on row ammount*/
        if (rows == 1) rowsPerColor = 1;
        else
        {
            if (rows % 3 == 0)
            {
                rowsPerColor = Mathf.RoundToInt(rows * 0.33f);
            }
            else
            {
                rowsPerColor = rows < 6 ? Mathf.RoundToInt(rows * 0.5f) : Mathf.RoundToInt(rows * 0.33f);
            }
        }

        float brickHalfWidth = brickPrefab.GetComponent<SpriteRenderer>().size.x * brickPrefab.transform.localScale.x;
        float brickHalfHeight = brickPrefab.GetComponent<SpriteRenderer>().size.y * brickPrefab.transform.localScale.y;

        float brickYSpawnOffset = Camera.main.orthographicSize - (brickHalfHeight * rows);

        float totalWidthHalf = brickHalfWidth * collums * 0.5f;
        float totalHeightHalf = brickHalfHeight * rows * 0.5f;

        /*to center the bricks we use negative half of the total width and
        height as our x and y and to make it appear on top of the screen we offset y*/
        Vector2 startPosition = new Vector2(-totalWidthHalf, -totalHeightHalf + brickYSpawnOffset);
        Color brickColor = Color.white;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < collums; x++)
            {
                //to create a margin between each brick we add brick margin
                Vector2 spawnPosition = startPosition + new Vector2(
                    x * brickHalfWidth + x * brickMargin,
                    y * brickHalfHeight + y * brickMargin);
                brickArray[x, y] = Instantiate(brickPrefab, spawnPosition, Quaternion.identity, transform);

                brickArray[x, y].GetComponent<BrickAttributes>().SetColor(brickColor);
            }
            if ((y + 1) % rowsPerColor == 0)
            {
                int rgbIndex = (int)(y / (float)rowsPerColor);
                brickColor[rgbIndex] = 0;
            }
        }
    }

    private void OnGameOver ()
    {
        for (int y = 0; y < brickArray.GetLength(1); y++)
        {
            for (int x = 0; x < brickArray.GetLength(0); x++)
            {
                Destroy(brickArray[x, y]);
            }
        }
        brickArray = null;
    }
}