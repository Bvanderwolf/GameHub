using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class GemBehaviour : MonoBehaviour
{
    private GemManager manager;
    private SpriteRenderer spriteRend;
    [SerializeField] private Vector2Int positionInList;

    public Vector2Int PositionInList => positionInList;
    public Action<GemBehaviour> OnRootSearchFailed;

    private readonly float explodeDelay = 0.1f;
    private readonly float explodeSpeed = 3f;
    private readonly float fadeSpeed = 1.5f;

    public bool exploding { get; private set; } = false;
    public bool exploded = false;
    public bool removingSelf { get; private set; } = false;

    [SerializeField] private bool usedForRootSearch = false;
    [SerializeField] private bool hasRootPath = false;

    private CircleCollider2D circleCollider;
    private Rigidbody2D body;
    private readonly float explodeRadius = 4.5f;
    private readonly float circleRadius = 2.6f;

    public string rootPath = "";

    private void Awake ()
    {
        spriteRend = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        body = GetComponent<Rigidbody2D>();
    }

    public void initializeWithManager (GemManager _manager, Vector2Int _position)
    {
        manager = _manager;
        positionInList = _position;
        OnRootSearchFailed += manager.RemoveGemFromList;
        CreateRootPath();
    }

    public void Recruit (GameObject recruitedGem)
    {
        if (manager != null)
        {
            recruitedGem.GetComponent<GemBehaviour>().initializeWithManager(
                manager,
                manager.RecruitGem(recruitedGem, this.gameObject));
        }
    }

    private void OnTriggerEnter2D (Collider2D collision)
    {
        if (collision.gameObject.tag == "Gem")
        {
            if (!body.isKinematic) Attach(collision);

            if (exploding)
            {
                Debug.Log($"exploding at {positionInList}");
                if (collision.gameObject.GetComponent<SpriteRenderer>().sprite == spriteRend.sprite)
                {
                    collision.gameObject.GetComponent<GemBehaviour>().Explode();
                }
            }
        }
    }

    //attaches gem to given collider
    private void Attach (Collider2D collision)
    {
        SetDefaultPhysicsValues();
        StartCoroutine(StartExplode());
        collision.gameObject.GetComponent<GemBehaviour>().Recruit(this.gameObject);
    }

    public void CreateRootPath ()
    {
        rootPath += $"{positionInList.x}:{positionInList.y}";
        if (positionInList.y != 0)
        {
            SearchSequential();
            if (!VerifyRootPath())
            {
                Debug.Log($"creating root path failed by gem with position {positionInList} :: destroying it");
                RemoveSelf(1);
                OnRootSearchFailed(this);
            }
            else ResetRootSearchValues();
        }
        else hasRootPath = true;
    }

    public void CheckOnRootPath ()
    {
        if (!VerifyRootPath())
        {
            rootPath = "";
            CreateRootPath();
        }
    }

    private void ResetRootSearchValues ()
    {
        string[] positions = rootPath.Split(',');
        foreach (string stringPos in positions)
        {
            string[] axis = stringPos.Split(':');
            Vector2Int pos = new Vector2Int(int.Parse(axis[0]), int.Parse(axis[1]));
            GemBehaviour behaviour = manager.GetGem(pos)?.GetComponent<GemBehaviour>();
            if (behaviour.usedForRootSearch) behaviour.usedForRootSearch = false;
        }
    }

    private bool VerifyRootPath ()
    {
        string[] positions = rootPath.Split(',');
        if (!positions.Any((s) => int.Parse(s.Split(':')[1]) == 0))
            return false;

        foreach (string stringPos in positions)
        {
            string[] axis = stringPos.Split(':');
            Vector2Int pos = new Vector2Int(int.Parse(axis[0]), int.Parse(axis[1]));
            GemBehaviour behaviour = manager.GetGem(pos)?.GetComponent<GemBehaviour>();
            if (behaviour == null)
            {
                Debug.Log($"root path of {positionInList} has null gem in it @ position: {pos}");
                return false;
            }
        }
        return true;
    }

    private void SearchSequential ()
    {
        string subRootPath = "";

        Vector2Int gemTopPosition = new Vector2Int(positionInList.x, positionInList.y - 1);
        GemBehaviour gemTopBehaviour = manager.GetGem(gemTopPosition)?.GetComponent<GemBehaviour>();
        if (gemTopBehaviour != null && !gemTopBehaviour.usedForRootSearch)
        {
            gemTopBehaviour.usedForRootSearch = true;
            if (SearchFrom(gemTopPosition, ref subRootPath))
            {
                rootPath += subRootPath;
                hasRootPath = true;
                return;
            }
            subRootPath = "";
        }
        bool isEvenRow = (positionInList.y + 1) % 2 == 0;
        Vector2Int gemTopSidePosition = new Vector2Int(isEvenRow ? positionInList.x - 1 : positionInList.x + 1, positionInList.y - 1);
        GemBehaviour gemTopSideBehaviour = manager.GetGem(gemTopSidePosition)?.GetComponent<GemBehaviour>();
        if (gemTopSideBehaviour != null && !gemTopSideBehaviour.usedForRootSearch)
        {
            gemTopSideBehaviour.usedForRootSearch = true;
            if (SearchFrom(gemTopSidePosition, ref subRootPath))
            {
                rootPath += subRootPath;
                hasRootPath = true;
                return;
            }
            subRootPath = "";
        }
        Vector2Int gemBotPosition = new Vector2Int(positionInList.x, positionInList.y + 1);
        GemBehaviour gemBotBehaviour = manager.GetGem(gemBotPosition)?.GetComponent<GemBehaviour>();
        if (gemBotBehaviour != null && !gemBotBehaviour.usedForRootSearch)
        {
            gemBotBehaviour.usedForRootSearch = true;
            if (SearchFrom(gemBotPosition, ref subRootPath))
            {
                rootPath += subRootPath;
                hasRootPath = true;
                return;
            }
            subRootPath = "";
        }
        Vector2Int gemBotSidePosition = new Vector2Int(positionInList.x + 1, positionInList.y + 1);
        GemBehaviour gemBotSideBehaviour = manager.GetGem(gemBotSidePosition)?.GetComponent<GemBehaviour>();
        if (gemBotSideBehaviour != null && !gemBotSideBehaviour.usedForRootSearch)
        {
            gemBotSideBehaviour.usedForRootSearch = true;
            if (SearchFrom(gemBotSidePosition, ref subRootPath))
            {
                rootPath += subRootPath;
                hasRootPath = true;
                return;
            }
            subRootPath = "";
        }
        Vector2Int gemLeftPosition = new Vector2Int(positionInList.x - 1, positionInList.y);
        GemBehaviour gemLeftBehaviour = manager.GetGem(gemLeftPosition)?.GetComponent<GemBehaviour>();
        if (gemLeftBehaviour != null && !gemLeftBehaviour.usedForRootSearch)
        {
            gemLeftBehaviour.usedForRootSearch = true;
            if (SearchFrom(gemLeftPosition, ref subRootPath))
            {
                rootPath += subRootPath;
                hasRootPath = true;
                return;
            }
            subRootPath = "";
        }
        Vector2Int gemRightPosition = new Vector2Int(positionInList.x + 1, positionInList.y);
        GemBehaviour gemRightBehaviour = manager.GetGem(gemRightPosition)?.GetComponent<GemBehaviour>();
        if (gemRightBehaviour != null && !gemRightBehaviour.usedForRootSearch)
        {
            gemRightBehaviour.usedForRootSearch = true;
            if (SearchFrom(gemRightPosition, ref subRootPath))
            {
                rootPath += subRootPath;
                hasRootPath = true;
                return;
            }
            subRootPath = "";
        }
    }

    private bool SearchFrom (Vector2Int fromPos, ref string subRootPath)
    {
        subRootPath += $",{fromPos.x}:{fromPos.y}";

        if (fromPos.y == 0) return true;

        Vector2Int gemTopPosition = new Vector2Int(fromPos.x, fromPos.y - 1);
        GemBehaviour gemTopBehaviour = manager.GetGem(gemTopPosition)?.GetComponent<GemBehaviour>();
        if (gemTopBehaviour != null && !gemTopBehaviour.usedForRootSearch)
        {
            gemTopBehaviour.usedForRootSearch = true;
            return (SearchFrom(gemTopPosition, ref subRootPath));
        }
        bool isEvenRow = (fromPos.y + 1) % 2 == 0;
        Vector2Int gemTopSidePosition = new Vector2Int(isEvenRow ? fromPos.x - 1 : fromPos.x + 1, fromPos.y - 1);
        GemBehaviour gemSideTopBehaviour = manager.GetGem(gemTopSidePosition)?.GetComponent<GemBehaviour>();
        if (gemSideTopBehaviour != null && !gemSideTopBehaviour.usedForRootSearch)
        {
            gemSideTopBehaviour.usedForRootSearch = true;
            return SearchFrom(gemTopSidePosition, ref subRootPath);
        }
        Vector2Int gemBotPosition = new Vector2Int(fromPos.x, fromPos.y + 1);
        GemBehaviour gemBotBehaviour = manager.GetGem(gemBotPosition)?.GetComponent<GemBehaviour>();
        if (gemBotBehaviour != null && !gemBotBehaviour.usedForRootSearch)
        {
            gemBotBehaviour.usedForRootSearch = true;
            return SearchFrom(gemBotPosition, ref subRootPath);
        }
        Vector2Int gemBotSidePosition = new Vector2Int(fromPos.x + 1, fromPos.y + 1);
        GemBehaviour gemBotSideBehaviour = manager.GetGem(gemBotSidePosition)?.GetComponent<GemBehaviour>();
        if (gemBotSideBehaviour != null && !gemBotSideBehaviour.usedForRootSearch)
        {
            gemBotSideBehaviour.usedForRootSearch = true;
            return SearchFrom(gemBotSidePosition, ref subRootPath);
        }
        Vector2Int gemLeftPosition = new Vector2Int(fromPos.x - 1, fromPos.y);
        GemBehaviour gemLeftBehaviour = manager.GetGem(gemLeftPosition)?.GetComponent<GemBehaviour>();
        if (gemLeftBehaviour != null && !gemLeftBehaviour.usedForRootSearch)
        {
            gemLeftBehaviour.usedForRootSearch = true;
            return SearchFrom(gemLeftPosition, ref subRootPath);
        }
        Vector2Int gemRightPosition = new Vector2Int(fromPos.x + 1, fromPos.y);
        GemBehaviour gemRightBehaviour = manager.GetGem(gemRightPosition)?.GetComponent<GemBehaviour>();
        if (gemRightBehaviour != null && !gemRightBehaviour.usedForRootSearch)
        {
            gemRightBehaviour.usedForRootSearch = true;
            return SearchFrom(gemRightPosition, ref subRootPath);
        }
        return false;
    }

    //starts explode coroutine if gem is not yet exploding
    public void Explode ()
    {
        if (!exploding) StartCoroutine(StartExplode());
    }

    //linearly interpolates radius of CircleCollider2D between current as start and explodeRadius as end
    private IEnumerator StartExplode ()
    {
        exploding = true;
        float currentLerpTime = 0;
        float radius = circleCollider.radius;
        while (currentLerpTime != 1)
        {
            currentLerpTime += Time.deltaTime * explodeSpeed;
            if (currentLerpTime > 1) currentLerpTime = 1;
            circleCollider.radius = radius + (currentLerpTime * (explodeRadius - radius));
            yield return null;
        }
        exploding = false;
        exploded = true;
        circleCollider.radius = circleRadius;
    }

    //sets default values of rigidbody and circlecollider2d
    public void SetDefaultPhysicsValues ()
    {
        body.velocity = Vector2.zero;
        body.isKinematic = true;
        circleCollider.radius = circleRadius;
        circleCollider.isTrigger = true;
    }

    //start FadeToDestroy courotine to remove itself based on given position In Exploding Gems sequence
    public void RemoveSelf (int positionInExplodingGems)
    {
        removingSelf = true;
        StartCoroutine(FadeToDestroy(positionInExplodingGems * explodeDelay));
    }

    //linearly interpolates alpha value of image color from 1 to 0 and then destroys gameobject
    private IEnumerator FadeToDestroy (float delay)
    {
        yield return new WaitForSeconds(delay);

        float currentLerpTime = 0;
        while (currentLerpTime != 1)
        {
            currentLerpTime += Time.deltaTime * fadeSpeed;
            if (currentLerpTime > 1) currentLerpTime = 1;

            float lerpedAlpha = 1 + (currentLerpTime * (0 - 1));
            spriteRend.color = new Color(1, 1, 1, lerpedAlpha);
            yield return null;
        }
        Destroy(this.gameObject);
    }
}