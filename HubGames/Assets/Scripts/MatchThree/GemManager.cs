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

    private MatchThreeUI userinterface;

    public enum AttachSide { LEFTBOT, LEFTMIDDLE, LEFTTOP, RIGHTBOT, RIGHTMIDDLE, RIGHTTOP }

    public readonly int MIN_CHAIN_COUNT = 3;

    public bool IsRootGem (GameObject gem) => GemSockets[0] != null && GemSockets[0].Any((socket) => socket.gem == gem);

    public List<List<GemSocket>> GemSockets { get; } = new List<List<GemSocket>>();

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

        float gemYSpawnOffset = Camera.main.orthographicSize - ((gemHalfHeight * rows) * 0.5f);

        totalWidthHalf = gemHalfWidth * MAX_START_GEMS_PER_ROW * 0.5f;
        totalHeightHalf = gemHalfHeight * rows * 0.5f;

        /*to center the gems we use negative half of the total width and
        height as our x and y and to make it appear on top of the screen we offset y*/
        Func<bool, Vector2> StartPosition = (_isEvenRow) =>
        {
            float x = _isEvenRow ? -totalWidthHalf : -totalWidthHalf + (gemHalfWidth * 0.5f);
            return new Vector2(
            x + (rows - 1) * GEM_MARGIN,
            -totalHeightHalf + gemYSpawnOffset + ((rows - 1) * gemHalfHeight) + ((rows - 1) * GEM_MARGIN));
        };

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
                gem.GetComponent<SpriteRenderer>().sprite = GetRandomGemSprite();
                gem.GetComponent<Rigidbody2D>().isKinematic = true;
                gem.GetComponent<GemBehaviour>().initialize(this);
            }
        }
        userinterface = FindObjectOfType<MatchThreeUI>();
        userinterface.UpdateVisuals(this);
    }

    private void AddNullRowToGemList ()
    {
        GemSockets.Add(new List<GemSocket>());
        bool isEven = GemSockets.Count % 2 == 0;
        int rows = isEven ? MAX_START_GEMS_PER_ROW : MAX_START_GEMS_PER_ROW - 1;
        float x = isEven ? -totalWidthHalf : -totalWidthHalf + (gemHalfWidth * 0.5f);
        float y = GemSockets[GemSockets.Count - 2][0].pos.y - gemHalfWidth - GEM_MARGIN;
        for (int i = 0; i < rows; i++)
        {
            GemSockets[GemSockets.Count - 1].Add(new GemSocket(null, new Vector2(x, y)));
        }
    }

    public void AddGemToList (GameObject gem, GameObject recruiter, AttachSide side)
    {
        int recruiterRow = -1;
        int recruiterCol = -1;
        for (int row = 0; row < GemSockets.Count; row++)
        {
            if (GemSockets[row].Any((socket) => socket.gem == recruiter))
            {
                recruiterRow = row;
                for (int col = 0; col < GemSockets[row].Count; col++)
                {
                    if (GemSockets[row][col].gem == recruiter) recruiterCol = col;
                }
            }
        }
        Debug.Log(side);
        Debug.Log(recruiterRow);

        if (side == AttachSide.LEFTTOP || side == AttachSide.RIGHTTOP)
        {
            if (recruiterRow == GemSockets.Count - 1)
                AddNullRowToGemList();

            if (recruiterRow != -1 && recruiterCol != -1)
            {
                int indexInRow = side == AttachSide.LEFTTOP ? recruiterCol - 1 : recruiterCol + 1;
                if (indexInRow < 0) indexInRow = 0;
                if (indexInRow >= GemSockets[recruiterRow + 1].Count) indexInRow = GemSockets[GemSockets.Count - 1].Count - 1;
                Vector2 socketPos = GemSockets[recruiterRow + 1][indexInRow].pos;
                GemSockets[recruiterRow + 1][indexInRow] = new GemSocket(gem, socketPos);
            }
            else
            {
                Debug.LogError("recruiter was not found in gem list");
            }
        }
        else if (side == AttachSide.LEFTBOT || side == AttachSide.RIGHTBOT)
        {
            if (recruiterRow != -1 && recruiterCol != -1)
            {
                int indexInRow = side == AttachSide.LEFTBOT ? recruiterCol - 1 : recruiterCol + 1;
                if (indexInRow < 0) indexInRow = 0;
                if (indexInRow >= GemSockets[recruiterRow - 1].Count) indexInRow = GemSockets[GemSockets.Count - 1].Count - 1;
                Vector2 socketPos = GemSockets[recruiterRow - 1][indexInRow].pos;
                GemSockets[recruiterRow - 1][indexInRow] = new GemSocket(gem, socketPos);
            }
            else
            {
                Debug.LogError("recruiter was not found in gem list");
            }
        }
        else
        {
            if (recruiterRow != -1 && recruiterCol != -1)
            {
                int indexInRow = side == AttachSide.LEFTMIDDLE ? recruiterCol - 1 : recruiterCol + 1;
                if (indexInRow < 0) indexInRow = 0;
                if (indexInRow >= GemSockets[recruiterRow].Count) indexInRow = GemSockets[GemSockets.Count - 1].Count - 1;
                Vector2 socketPos = GemSockets[recruiterRow][indexInRow].pos;
                GemSockets[recruiterRow][indexInRow] = new GemSocket(gem, socketPos);
            }
            else
            {
                Debug.LogError("recruiter was not found in gem list");
            }
        }
        userinterface.UpdateVisuals(this);
    }

    [ContextMenu("test")]
    public void Test ()
    {
        foreach (List<GemSocket> list in GemSockets)
        {
            foreach (GemSocket s in list)
            {
                Debug.Log(s.gem);
            }
        }
    }

    public void RecruitGem (GameObject gem, GameObject recruiter)
    {
        float dotHorizontal = Vector2.Dot(recruiter.transform.position - gem.transform.position, recruiter.transform.right);
        float dotVertical = Vector2.Dot(recruiter.transform.position - gem.transform.position, Vector2.up);

        if (dotHorizontal <= 0)
        {
            if (dotVertical < -0.2f)
            {
                gem.transform.position = recruiter.transform.position + new Vector3(
               (gemHalfWidth * 0.5f) + GEM_MARGIN,
               gemHalfHeight + GEM_MARGIN);
                AddGemToList(gem, recruiter, AttachSide.RIGHTBOT);
            }
            else if (dotVertical > 0.2f)
            {
                gem.transform.position = recruiter.transform.position + new Vector3(
                (gemHalfWidth * 0.5f) + GEM_MARGIN,
                -(gemHalfHeight + GEM_MARGIN));
                AddGemToList(gem, recruiter, AttachSide.RIGHTTOP);
            }
            else
            {
                gem.transform.position = recruiter.transform.position + new Vector3(
               gemHalfWidth + GEM_MARGIN, 0);
                AddGemToList(gem, recruiter, AttachSide.RIGHTMIDDLE);
            }
        }
        else
        {
            if (dotVertical < -0.2f)
            {
                gem.transform.position = recruiter.transform.position + new Vector3(
                -((gemHalfWidth * 0.5f) + GEM_MARGIN),
                gemHalfHeight + GEM_MARGIN);
                AddGemToList(gem, recruiter, AttachSide.LEFTBOT);
            }
            else if (dotVertical > 0.2f)
            {
                gem.transform.position = recruiter.transform.position + new Vector3(
                -((gemHalfWidth * 0.5f) + GEM_MARGIN),
                -(gemHalfHeight + GEM_MARGIN));
                AddGemToList(gem, recruiter, AttachSide.LEFTTOP);
            }
            else
            {
                gem.transform.position = recruiter.transform.position + new Vector3(
                -(gemHalfWidth + GEM_MARGIN), 0);
                AddGemToList(gem, recruiter, AttachSide.LEFTMIDDLE);
            }
        }
    }

    public static Sprite GetRandomGemSprite ()
    {
        return gemSpriteDict[gemNames[Random.Range(0, gemNames.Length)]];
    }

    public struct GemSocket
    {
        public GameObject gem;
        public Vector2 pos;

        public GemSocket (GameObject _gem, Vector2 _pos)
        {
            gem = _gem;
            pos = _pos;
        }
    }
}