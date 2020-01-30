using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using SnackType = SnackSpawning.SnackType;

public class SnakeController : MonoBehaviour
{
    [SerializeField] private int start_Partcount;
    [SerializeField] private GameObject part;

    private List<GameObject> parts = new List<GameObject>();
    private List<GrowPoint> growPoints = new List<GrowPoint>();

    [SerializeField] private Vector2Int gridPosition;
    [SerializeField] private Vector2Int gridDirection;
    private Vector2Int lastTailPosition;

    private SnakeGrid grid;
    private AudioClip eatSound;
    private AudioSource audioSource;

    private int moveDimension = 0;
    private bool canTurn = true;

    private readonly float movedelay = 0.2f;
    private readonly uint timedSnackGrowth = 2;
    private uint snackGrowth = 1;

    private readonly string defaultMoveAnimName = "partmove500x500";
    public readonly int spawnMargin = 6;

    private int partAnimIndex;

    private Timer snackTimer;

    [SerializeField] private float timer;

    public int StartPartCount
    {
        get
        {
            return start_Partcount;
        }
        set
        {
            //start part count can be influenced only if the value is not greater than SnakeGrid it's spawnmargin
            if (value <= spawnMargin)
            {
                start_Partcount = value;
            }
        }
    }

    public event Action OnSelfCollision;

    public event Action OnSnackCollision;

    public event Action OnTimedSnackMissed;

    public void Init (SnakeGrid instance, Vector2Int _myPosition)
    {
        grid = instance;
        parts.Add(this.gameObject);

        eatSound = ResourceManager.GetResource<AudioClip>("snakeEat");
        partAnimIndex = GetDefaultPartAnimationClipIndex();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        InputSystem.Instance.OnDirectionKeyDown += OnDirectionChange;

        //we initialize the body of the snake
        InitBody(instance, _myPosition);

        //we add a growing point with "start_Partcount" count to it
        //making sure the snake has the right starting length
        growPoints.Add(new GrowPoint((uint)start_Partcount, _myPosition));

        //we start moving the snake
        StartCoroutine(MoveEnumerator());
    }

    private void SetAnimationClipValues (Animator animator)
    {
        AnimatorOverrideController aoc = new AnimatorOverrideController(animator.runtimeAnimatorController);
        aoc[defaultMoveAnimName] = animator.runtimeAnimatorController.animationClips[partAnimIndex];
        animator.runtimeAnimatorController = aoc;
    }

    private int GetDefaultPartAnimationClipIndex ()
    {
        string resolution = $"{Screen.width}x{Screen.height}";
        switch (resolution)
        {
            case "500x500": return 0;
            case "1000x1000": return 1;
            case "1920x1080": return 2;
            default: return -1;
        }
    }

    private void OnDestroy ()
    {
        InputSystem.Instance.OnDirectionKeyDown -= OnDirectionChange;
        StopAllCoroutines();
    }

    private void OnDirectionChange (KeyCode key)
    {
        //to make sure players can't move inside themself you can only turn after you have moved one place in the grid
        if (!canTurn)
            return;

        //we check on movedimension (0 = x and 1 = y) before we apply the on key functionalities
        if (moveDimension == 0)
        {
            if (key == KeyCode.UpArrow || key == KeyCode.W) { ChangeOrientation(270); }
            else if (key == KeyCode.DownArrow || key == KeyCode.S) { ChangeOrientation(90); }
        }
        else
        {
            if (key == KeyCode.LeftArrow || key == KeyCode.A) { ChangeOrientation(0); }
            else if (key == KeyCode.RightArrow || key == KeyCode.D) { ChangeOrientation(180); }
        }
    }

    //Changes orientation of snake by given degrees. This by changing the snakeHead, gridDirection and moveDimension
    private void ChangeOrientation (float degrees)
    {
        transform.localEulerAngles = new Vector3(0, 0, degrees);
        Vector3 direction = transform.rotation * -transform.right;
        gridDirection = MapDegreesToDirection(Mathf.RoundToInt(degrees / 90));
        moveDimension = moveDimension == 1 ? 0 : 1;
        canTurn = false;
    }

    /// <summary>
    /// returns a direction based on one in 4 degree types (0 || 90 || 180 || 270) / 90
    /// </summary>
    /// <param name="oneInFour"></param>
    /// <returns></returns>
    private Vector2Int MapDegreesToDirection (int oneInFour)
    {
        switch (oneInFour)
        {
            case 0: return new Vector2Int(-1, 0);
            case 1: return new Vector2Int(0, -1);
            case 2: return new Vector2Int(1, 0);
            case 3: return new Vector2Int(0, 1);
            default: return new Vector2Int();
        }
    }

    //movement is done by using an enumarator that moves the snake along the grid with "MOVEDELAY" delay in seconds
    private IEnumerator MoveEnumerator ()
    {
        //while the game is not over the snake moves and tries growing
        while (!SnakeGameState.GameOver)
        {
            Move();
            TryGrowBody();
            yield return new WaitForSeconds(movedelay);
            canTurn = true;
        }
    }

    private void Update ()
    {
        if (snackTimer != null)
        {
            timer = snackTimer.RemainingTime;
            snackTimer.Tick(Time.deltaTime);
        }
    }

    /// <summary>
    /// tries growing the body looking at the growing points list
    /// </summary>
    private void TryGrowBody ()
    {
        //if there are no growing points or any with last tail position in the list we return
        if (growPoints.Count == 0 || !growPoints.Any((gp) => gp.Position == lastTailPosition))
            return;

        //we get the index of the growing point and spawn a part at its position
        int index = growPoints.IndexOf(growPoints.Find((gp) => gp.Position == lastTailPosition));
        SpawnPartAt(grid.GetGridPosition(growPoints[index].Position));

        //if the growing point doesn't have any more to add we remove it from the list
        growPoints[index].DecreaseCount();
        if (growPoints[index].Empty)
        {
            growPoints.RemoveAt(index);
        }
    }

    /// <summary>
    /// sets up a part at given position adding it to the parts list
    /// </summary>
    /// <param name="position"></param>
    private void SpawnPartAt (Vector2 position)
    {
        GameObject partObj = Instantiate(part, position, Quaternion.identity, grid.transform);
        grid.SetWidthHeightOfPart(partObj);
        SetAnimationClipValues(partObj.GetComponent<Animator>());
        parts.Add(partObj);
    }

    private void Move ()
    {
        if (!grid)
            return;

        //save tail position
        GameObject tail = parts[parts.Count - 1];
        lastTailPosition = grid.GetGridPosition(tail.transform.position);

        //the snake moves along the grid by adding its direction to its current grid position
        Vector2 nextPosition = grid.GetGridPosition(gridPosition + gridDirection);
        gridPosition = grid.GetGridPosition(nextPosition);

        //al parts their position is changed based on the part before it in the list
        for (int i = parts.Count - 1; i >= 1; i--)
        {
            parts[i].transform.position = parts[i - 1].transform.position;
        }
        transform.position = nextPosition;

        //after all movement is finished we check for collisions
        CheckForCollisions();
    }

    private void CheckForCollisions ()
    {
        //if any of the parts (excluding the head) have the same position as the head it means the snake is colliding with itself
        if (parts.Any((go) => go.name != gameObject.name && go.transform.position == transform.position))
        {
            //if the snake collides with itself we stop all functions of this script, delete each part and raise an event
            InputSystem.Instance.OnDirectionKeyDown -= OnDirectionChange;
            StopAllCoroutines();
            OnSelfCollision?.Invoke();
            for (int i = parts.Count - 1; i >= 0; i--)
            {
                Destroy(parts[i].gameObject);
            }
            return;
        }
        //if any of the parts (including the head) is on the same position as the target it means we collided with a snack
        if (parts.Any((go) => go.transform.position == grid.CurrentSnackPosition))
        {
            EatSnack();
        }
    }

    private void EatSnack ()
    {
        //when eating a snack we add a growing point at the end of the snake and throw an event
        AddGrowtPoints();
        CheckForSnackTimerReset();
        audioSource.PlayOneShot(eatSound);
        OnSnackCollision?.Invoke();
        CheckForTimedSnackSpawn();
    }

    //checks if the newly spawned snack is a timed snack
    private void CheckForTimedSnackSpawn ()
    {
        if (grid.CurrentSnackType == SnackType.TIMED)
        {
            int diffX = Mathf.Abs(grid.CurrentSnackGridPosition.x - gridPosition.x);
            int diffY = Mathf.Abs(grid.CurrentSnackGridPosition.y - gridPosition.y);
            int stepCount = diffX + diffY;
            float offset = movedelay * 0.5f;
            float duration = stepCount * movedelay + offset;
            Debug.Log(duration);
            snackTimer = new Timer(duration, () =>
            {
                snackTimer = null;
                OnTimedSnackMissed();
            });
        }
    }

    private void CheckForSnackTimerReset ()
    {
        if (snackTimer != null)
        {
            snackTimer = null;
        }
    }

    private void AddGrowtPoints ()
    {
        uint growthCount = GetGrowth();
        GameObject tail = parts[parts.Count - 1];
        growPoints.Add(new GrowPoint(growthCount, grid.CurrentSnackGridPosition));
    }

    private uint GetGrowth ()
    {
        if (grid.CurrentSnackType == SnackType.SPECIAL)
        {
            return snackGrowth + 1;
        }
        if (grid.CurrentSnackType == SnackType.TIMED)
        {
            return snackGrowth + timedSnackGrowth;
        }
        return snackGrowth;
    }

    private void InitBody (SnakeGrid instance, Vector2Int _myPosition)
    {
        //set position in grid
        gridPosition = _myPosition;

        //set width and height based on screen resolution
        grid.SetWidthHeightOfPart(this.gameObject);

        //set orientation and position
        int oneInFour = Random.Range(0, 5);
        transform.rotation = Quaternion.Euler(0, 0, 90 * oneInFour);
        transform.position = instance.gridPositions[_myPosition.x, _myPosition.y];

        //get lookdirection of snake and base of that the position of the parts behind the snake
        Vector3 inverseDirection = transform.rotation * Vector3.right;
        Vector2Int inverseGridDirection = new Vector2Int(Mathf.RoundToInt(inverseDirection.x), Mathf.RoundToInt(inverseDirection.y));

        //store the actual grid direction and move dimension (x = 0, and y = 1)
        gridDirection = new Vector2Int(-inverseGridDirection.x, -inverseGridDirection.y);
        moveDimension = (gridDirection.x == 1 || gridDirection.x == -1) ? 0 : 1;
    }

    private class GrowPoint
    {
        public uint Count { get; private set; }
        public readonly Vector2Int Position;

        public bool Empty => Count == 0;

        public GrowPoint (uint _count, Vector2Int _position)
        {
            Count = _count;
            Position = _position;
        }

        public void DecreaseCount ()
        {
            if (Count > 0) Count--;
        }
    }
}