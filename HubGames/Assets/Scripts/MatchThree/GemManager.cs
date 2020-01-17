using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GemManager : MonoBehaviour
{
    [SerializeField] private GameObject gemPrefab;
    [SerializeField] private Transform topBound;
    [SerializeField] private int rows;

    public int Rows => rows;

    public readonly int MAX_START_GEMS_PER_ROW = 11;
    private readonly float GEM_MARGIN = 0.04f;
    private static string[] gemNames = new string[] { "blue", "green", "red", "yellow" };
    private static Dictionary<string, Sprite> gemSpriteDict = new Dictionary<string, Sprite>();

    private float gemHalfWidth;
    private float gemHalfHeight;
    private float totalWidthHalf;
    private float totalHeightHalf;

    public readonly int MIN_EXPLODE_COUNT = 3;
    private readonly float MIDDLE_ATTACH_RANGE = 0.3f;

    public bool IsRootGem (GameObject gem) => GemSockets[0] != null && GemSockets[0].Any((socket) => socket.gem == gem);

    public List<List<GemSocket>> GemSockets { get; } = new List<List<GemSocket>>();

    private bool explodingGems = false;

    private void Awake ()
    {
        gemSpriteDict.Add(gemNames[0], ResourceManager.GetResource<Sprite>("gemBlue"));
        gemSpriteDict.Add(gemNames[1], ResourceManager.GetResource<Sprite>("gemGreen"));
        gemSpriteDict.Add(gemNames[2], ResourceManager.GetResource<Sprite>("gemRed"));
        gemSpriteDict.Add(gemNames[3], ResourceManager.GetResource<Sprite>("gemYellow"));
    }

    // Start is called before the first frame update
    private void Start ()
    {
        gemHalfWidth = gemPrefab.GetComponent<SpriteRenderer>().size.x * gemPrefab.transform.localScale.x;
        gemHalfHeight = gemPrefab.GetComponent<SpriteRenderer>().size.y * gemPrefab.transform.localScale.y;

        float gemYSpawnOffset = Camera.main.orthographicSize - (gemHalfHeight * rows);

        totalWidthHalf = gemHalfWidth * MAX_START_GEMS_PER_ROW * 0.5f;

        /*To make the gems start spawning on the left top of the screen we set the y axis start
         to the camera size minus combined height of gems halfed adding the position for a gem
         on the top row*/
        Func<bool, Vector2> StartPosition = (_isEvenRow) =>
        {
            float x = _isEvenRow ? -totalWidthHalf : -totalWidthHalf + (gemHalfWidth * 0.5f);
            return new Vector2(
            x + (rows - 1) * GEM_MARGIN,
            gemYSpawnOffset + ((rows - 1) * gemHalfHeight) + ((rows - 1) * GEM_MARGIN));
        };

        //The gems are spawned from the top of the screen going down because gems are added
        //by player shooting on the bottom.
        for (int row = 0; row < rows; row++)
        {
            GemSockets.Add(new List<GemSocket>());
            //if row + 1 is an even number we add MAX_GEMS_PER_ROW gems, otherwise the same - 1
            bool isEven = (row + 1) % 2 == 0;
            int cols = isEven ? MAX_START_GEMS_PER_ROW : MAX_START_GEMS_PER_ROW - 1;

            for (int col = 0; col < cols; col++)
            {
                GameObject gem = Instantiate(
                    gemPrefab,
                    StartPosition(isEven) + new Vector2(
                        (col * gemHalfWidth) + (col * GEM_MARGIN),
                        -(row * gemHalfHeight) - (row * GEM_MARGIN)),
                    Quaternion.identity,
                    transform
                    );
                GemSockets[row].Add(new GemSocket(gem, gem.transform.position));
                //added gems get a random gem sprite, are set the be kinematic and then initialized internaly
                gem.GetComponent<SpriteRenderer>().sprite = GetRandomGemSprite();
                GemBehaviour behaviour = gem.GetComponent<GemBehaviour>();
                behaviour.initializeWithManager(this, new Vector2Int(col, row));
                behaviour.SetDefaultPhysicsValues();
            }
        }
    }

    private void FixedUpdate ()
    {
        //if there are gems exploding (defined by "explodingGems") we observe the gemsocket list
        if (explodingGems) ObserveExplodingGems();
    }

    private void StartObservingExplodingGems ()
    {
        explodingGems = true;
    }

    //looks at all gems in sockets and checks if all gems that are exploding are finished or not
    private void ObserveExplodingGems ()
    {
        //define whether there are still gems left that are exploding but haven't exploded fully
        bool anyGemsAreExploding = GemSockets.Any((list) => list.Any((socket) =>
        {
            GemBehaviour behaviour = socket.gem?.GetComponent<GemBehaviour>();
            return behaviour != null && behaviour.exploding && !behaviour.exploded;
        }));

        //if there are no gems left that are exploding but not fully we can count them and see if they can be removed
        if (!anyGemsAreExploding)
        {
            //add all exploded gems toghether
            int explodeCount = 0;
            foreach (List<GemSocket> list in GemSockets)
            {
                explodeCount += list.Count((socket) =>
                {
                    GemBehaviour behaviour = socket.gem?.GetComponent<GemBehaviour>();
                    return behaviour != null && behaviour.exploded;
                });
            }

            //if the explodeCount is high enough we can remove the gems that have exploded,
            if (explodeCount >= MIN_EXPLODE_COUNT)
            {
                //for each gem exploded we let it remove itself and update the gemSocket list,
                //currentRecruit value and explodeCount value accordingly
                foreach (List<GemSocket> list in GemSockets)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        GemBehaviour behaviour = list[i].gem?.GetComponent<GemBehaviour>();
                        if (behaviour != null && behaviour.exploded)
                        {
                            behaviour.RemoveSelf(explodeCount);
                            behaviour.OnRootSearchFailed -= RemoveGemFromList;
                            list[i] = new GemSocket(null, behaviour.transform.position);
                            explodeCount--;
                        }
                    }
                }

                //after removing the gems from the GemSocket list we need to check the correctness of the rootpath for each gem
                //maybe removing some in the process because of no valid root path
                for (int row = 0; row < GemSockets.Count; row++)
                {
                    for (int col = 0; col < GemSockets[row].Count; col++)
                    {
                        GemBehaviour behaviour = GemSockets[row][col].gem?.GetComponent<GemBehaviour>();
                        if (behaviour != null) behaviour.CheckOnRootPath();
                    }
                }
            }
            else
            {
                //if the ammount of gems wasn't enough for destruction we reset their "exploded" value
                GemSockets.ForEach((list) => list.ForEach((socket) =>
                {
                    GemBehaviour behaviour = socket.gem?.GetComponent<GemBehaviour>();
                    if (behaviour != null && behaviour.exploded) behaviour.exploded = false;
                }));
            }

            //we stop observing gems by setting "explodingGems" to false
            explodingGems = false;
        }
    }

    //adds a row to the gemsocket list with all GemSocket Wrappers theirs gameobject reference set to null.
    private void AddNullRowToGemList ()
    {
        GemSockets.Add(new List<GemSocket>());
        bool isEven = GemSockets.Count % 2 == 0;
        int rows = isEven ? MAX_START_GEMS_PER_ROW : MAX_START_GEMS_PER_ROW - 1;
        Func<int, float> x = (row) =>
        {
            return isEven ? -totalWidthHalf + (row * gemHalfWidth) + (row * GEM_MARGIN)
             : -totalWidthHalf + (gemHalfWidth * 0.65f) + (row * gemHalfWidth) + (row * GEM_MARGIN);
        };
        float y = GemSockets[GemSockets.Count - 2][0].pos.y - gemHalfWidth - GEM_MARGIN;
        for (int row = 0; row < rows; row++)
        {
            GemSockets[GemSockets.Count - 1].Add(new GemSocket(null, new Vector2(x(row), y)));
        }
    }

    //returns position of gem in the GemSockets list in vector2int form.
    //returns -1 for axis if not found.
    private Vector2Int GetPositionInList (GameObject _gem)
    {
        int recruiterRow = -1;
        int recruiterCol = -1;
        for (int row = 0; row < GemSockets.Count; row++)
        {
            if (GemSockets[row].Any((socket) => socket.gem == _gem))
            {
                recruiterRow = row;
                for (int col = 0; col < GemSockets[row].Count; col++)
                {
                    if (GemSockets[row][col].gem == _gem) recruiterCol = col;
                }
            }
        }
        return new Vector2Int(recruiterCol, recruiterRow);
    }

    public Vector2Int AddGemToList (GameObject gem, GameObject recruiter, int side)
    {
        //get position of recruiter in list and return error if not found
        Vector2Int recruiterPos = GetPositionInList(recruiter);
        if (recruiterPos.x == -1 || recruiterPos.y == -1)
        {
            Debug.LogError("recruiter was not found in gem list");
            return new Vector2Int();
        }

        //if the recruiter is found we setup the variables necessary for finding a position for recruited gem
        Vector2 socketPos = new Vector2();
        bool isEvenRow = (recruiterPos.y + 1) % 2 == 0;
        int recruitPosX = 0;
        int recruitPosY = 0;

        /*if the recruited gem was recruited on top of a recruiter and this recruiter was
         on the top row of the GemSockets list we add a new row to it so it can fit*/
        if (RelativePositions.TOP(side) && recruiterPos.y == GemSockets.Count - 1)
        {
            AddNullRowToGemList();
        }

        /*If a gem is recruited on the top or bottom of a recruiter we need to check
         left vs right, top vs bottom and then if recruited row as an even row. Based on
         these variables we change the recruit position of the gem. If the gem was recruited
         by attaching in the middle we can just check left vs right and set the
         recruit y pos to the recruit y position. */
        if (!RelativePositions.MIDDLE(side))
        {
            if (RelativePositions.LEFT(side))
            {
                recruitPosX = recruiterPos.x - 1;
                recruitPosY = RelativePositions.TOP(side) ? recruiterPos.y + 1 : recruiterPos.y - 1;
                if (!isEvenRow) recruitPosX++;
            }
            else if (RelativePositions.RIGHT(side))
            {
                recruitPosX = recruiterPos.x + 1;
                recruitPosY = RelativePositions.TOP(side) ? recruiterPos.y + 1 : recruiterPos.y - 1;
                if (isEvenRow) recruitPosX--;
            }
        }
        else
        {
            recruitPosX = side == RelativePositions.LEFTMIDDLE ? recruiterPos.x - 1 : recruiterPos.x + 1;
            recruitPosY = recruiterPos.y;
        }

        //we make sure the recruitPos is not out of bounds *Problem: exceptions override other gems which isnt intended
        if (recruitPosX < 0) recruitPosX = 0;
        if (recruitPosX >= GemSockets[recruitPosY].Count) recruitPosX = GemSockets[recruitPosY].Count - 1;

        //set position of gem to a socketposition, set parent to this transform and start observing for exploding gems
        socketPos = GemSockets[recruitPosY][recruitPosX].pos;
        GemSockets[recruitPosY][recruitPosX] = new GemSocket(gem, socketPos);
        gem.transform.position = socketPos;
        gem.transform.parent = transform;

        StartObservingExplodingGems();

        //Debug.Log($"recruitPos: {recruiterPos}\n recruitPosX: {recruitPosX}\n recruitPosY: {recruitPosY}\n side: {side}");
        return new Vector2Int(recruitPosX, recruitPosY);
    }

    //recruits gem given gem to recruit and recruiter gem
    public Vector2Int RecruitGem (GameObject gem, GameObject recruiter)
    {
        //dothorizontal defines whether the gem landed left or right of the recruiter and
        //dotvertical defines whether the gem landed on top or bottom of the recruiter
        float dotHorizontal = Vector2.Dot(recruiter.transform.position - gem.transform.position, recruiter.transform.right);
        float dotVertical = Vector2.Dot(recruiter.transform.position - gem.transform.position, Vector2.up);

        if (dotHorizontal <= 0)
        {
            if (dotVertical < -MIDDLE_ATTACH_RANGE)
            {
                return AddGemToList(gem, recruiter, RelativePositions.RIGHTBOT);
            }
            else if (dotVertical > MIDDLE_ATTACH_RANGE)
            {
                return AddGemToList(gem, recruiter, RelativePositions.RIGHTTOP);
            }
            else
            {
                return AddGemToList(gem, recruiter, RelativePositions.RIGHTMIDDLE);
            }
        }
        else
        {
            if (dotVertical < -MIDDLE_ATTACH_RANGE)
            {
                return AddGemToList(gem, recruiter, RelativePositions.LEFTBOT);
            }
            else if (dotVertical > MIDDLE_ATTACH_RANGE)
            {
                return AddGemToList(gem, recruiter, RelativePositions.LEFTTOP);
            }
            else
            {
                return AddGemToList(gem, recruiter, RelativePositions.LEFTMIDDLE);
            }
        }
    }

    public void RemoveGemFromList (GemBehaviour behaviour)
    {
        behaviour.OnRootSearchFailed -= RemoveGemFromList;
        Vector2 pos = behaviour.transform.position;
        GemSockets[behaviour.PositionInList.y][behaviour.PositionInList.x] = new GemSocket(null, pos);
    }

    /// <summary>
    /// returns random sprite from gem sprite dictionary
    /// </summary>
    /// <returns></returns>
    public static Sprite GetRandomGemSprite ()
    {
        return gemSpriteDict[gemNames[Random.Range(0, gemNames.Length)]];
    }

    public GameObject GetGem (Vector2Int position)
    {
        if (position.y >= 0 && position.y < GemSockets.Count
        && position.x >= 0 && position.x < GemSockets[position.y].Count)
        {
            return GemSockets[position.y][position.x].gem;
        }
        else return null;
    }
}