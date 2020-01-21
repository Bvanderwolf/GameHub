using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public class SnakeController : MonoBehaviour
{
    [SerializeField] private int start_Partcount;
    [SerializeField] private GameObject part;

    private List<GameObject> parts = new List<GameObject>();

    [SerializeField] private Vector2Int gridPosition;
    [SerializeField] private Vector2Int gridDirection;

    private SnakeGrid grid;
    private AudioClip eatSound;
    private AudioSource audioSource;

    private int moveDimension = 0;
    private bool canTurn = true;

    private const float MOVEDELAY = 0.2f;

    private readonly string defaultMoveAnimName = "partmove500x500";
    private int partAnimIndex;

    public int StartPartCount
    {
        get
        {
            return start_Partcount;
        }
        set
        {
            //start part count can be influenced only if the value is not greater than SnakeGrid it's spawnmargin
            if (value <= SnakeGrid.SPAWNMARGIN)
            {
                start_Partcount = value;
            }
        }
    }

    public event Action OnSelfCollision;

    public event Action OnTargetCollision;

    public void Init (SnakeGrid instance, Vector2[,] gridPositions, Vector2Int _myPosition)
    {
        grid = instance;
        parts.Add(this.gameObject);
        grid.SetWidthHeightOfPart(this.gameObject);

        int oneInFour = Random.Range(0, 5);
        transform.rotation = Quaternion.Euler(0, 0, 90 * oneInFour);

        eatSound = ResourceManager.GetResource<AudioClip>("snakeEat");
        partAnimIndex = GetDefaultPartAnimationClipIndex();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        InputSystem.Instance.OnDirectionKeyDown += OnDirectionChange;

        CreateBody(instance, _myPosition);
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

    //movement is doen by using an enumarator that moves the snake along the grid with "MOVEDELAY" delay in seconds
    private IEnumerator MoveEnumerator ()
    {
        while (!SnakeGameState.GameOver)
        {
            Move();
            yield return new WaitForSeconds(MOVEDELAY);
            canTurn = true;
        }
    }

    private void Move ()
    {
        if (grid)
        {
            //the snake moves along the grid by adding its direction to its current grid position
            gridPosition += gridDirection;
            Vector2 nextPosition = grid.GetGridPosition(ref gridPosition);

            //al parts their position is changed based on the part before it in the list
            for (int i = parts.Count - 1; i >= 1; i--)
            {
                parts[i].transform.position = parts[i - 1].transform.position;
            }
            transform.position = nextPosition;

            //after all movement is finished we check for collisions
            CheckForCollisions();
        }
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
        }
        //if any of the parts (including the head) is on the same position as the target it means we collided with a snack
        if (parts.Any((go) => go.transform.position == grid.SnakeTargetPosition))
        {
            EatSnack();
        }
    }

    private void EatSnack ()
    {
        //when we eat a snack a new part is added based on the direction the snake is going and an event is raised
        Vector2Int lastPartPosition = grid.GetGridPosition(parts[parts.Count - 1].transform.position);
        Vector2Int partGridPosition = lastPartPosition - gridDirection;
        Vector2 partPosition = grid.GetGridPosition(ref partGridPosition);
        GameObject partObj = Instantiate(part, partPosition, Quaternion.identity, grid.transform);

        grid.SetWidthHeightOfPart(partObj);
        SetAnimationClipValues(partObj.GetComponent<Animator>());
        parts.Add(partObj);
        audioSource.PlayOneShot(eatSound);
        OnTargetCollision?.Invoke();
    }

    private void CreateBody (SnakeGrid instance, Vector2Int _myPosition)
    {
        gridPosition = _myPosition;

        //get lookdirection of snake and base of that the position of the parts behind the snake
        Vector3 inverseDirection = transform.rotation * Vector3.right;
        Vector2Int inverseGridDirection = new Vector2Int(Mathf.RoundToInt(inverseDirection.x), Mathf.RoundToInt(inverseDirection.y));

        //store the actual grid direction and move dimension (x = 0, and y = 1)
        gridDirection = new Vector2Int(-inverseGridDirection.x, -inverseGridDirection.y);
        moveDimension = (gridDirection.x == 1 || gridDirection.x == -1) ? 0 : 1;

        //add snake parts to grid based on inverseDirection
        for (int current = 0; current < start_Partcount; current++)
        {
            Vector2Int partGridPosition = gridPosition + inverseGridDirection * (current + 1);
            Vector2 partPosition = instance.GetGridPosition(ref partGridPosition);
            GameObject partObj = Instantiate(part, partPosition, Quaternion.identity, instance.gameObject.transform);
            grid.SetWidthHeightOfPart(partObj);
            SetAnimationClipValues(partObj.GetComponent<Animator>());
            parts.Add(partObj);
        }
    }
}