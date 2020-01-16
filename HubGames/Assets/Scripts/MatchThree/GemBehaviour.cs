using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GemBehaviour : MonoBehaviour
{
    private GemManager manager;
    private SpriteRenderer spriteRend;
    [SerializeField] private Vector2Int positionInList;

    public Vector2Int PositionInList => positionInList;

    private readonly float explodeDelay = 0.1f;
    private readonly float explodeSpeed = 3f;
    private readonly float fadeSpeed = 1.5f;

    public bool exploding { get; private set; } = false;
    public bool exploded = false;

    public bool usedForRootSearch { get; private set; } = false;

    private CircleCollider2D circleCollider;
    private Rigidbody2D body;
    private readonly float explodeRadius = 4.5f;
    private readonly float circleRadius = 2.6f;

    public string rootPath = "";
    public bool hasRootPath = false;

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

    private void CreateRootPath ()
    {
        rootPath += $"{positionInList.x}:{positionInList.y}";
        if (positionInList.y != 0)
        {
            SearchSequential();
        }
    }

    private void SearchSequential ()
    {
        string subRootPath = "";

        Vector2Int gemTopPosition = new Vector2Int(positionInList.x, positionInList.y - 1);
        GameObject gemTop = manager.GetGem(gemTopPosition);
        if (gemTop != null && !gemTop.GetComponent<GemBehaviour>().usedForRootSearch)
        {
            if (Search(gemTopPosition, ref subRootPath))
            {
                rootPath += subRootPath;
                hasRootPath = true;
                return;
            }
        }
        bool isEvenRow = (positionInList.y + 1) % 2 == 0;
        Vector2Int gemTopSidePosition = new Vector2Int(isEvenRow ? positionInList.x - 1 : positionInList.x + 1, positionInList.y - 1);
        GameObject gemSideTop = manager.GetGem(gemTopSidePosition);
        if (gemSideTop != null && !gemSideTop.GetComponent<GemBehaviour>().usedForRootSearch)
        {
            if (Search(gemTopSidePosition, ref subRootPath))
            {
                rootPath += subRootPath;
                hasRootPath = true;
                return;
            }
        }
        Vector2Int gemBotPosition = new Vector2Int(positionInList.x, positionInList.y + 1);
        GameObject gemBot = manager.GetGem(gemBotPosition);
        if (gemBot != null && !gemBot.GetComponent<GemBehaviour>().usedForRootSearch)
        {
            if (Search(gemBotPosition, ref subRootPath))
            {
                rootPath += subRootPath;
                hasRootPath = true;
                return;
            }
        }
        Vector2Int gemBotSidePosition = new Vector2Int(positionInList.x + 1, positionInList.y + 1);
        GameObject gemBotSide = manager.GetGem(gemBotSidePosition);
        if (gemBotSide != null && !gemBotSide.GetComponent<GemBehaviour>().usedForRootSearch)
        {
            if (Search(gemTopSidePosition, ref subRootPath))
            {
                rootPath += subRootPath;
                hasRootPath = true;
                return;
            }
        }
        Vector2Int gemLeftPosition = new Vector2Int(positionInList.x - 1, positionInList.y);
        GameObject gemLeft = manager.GetGem(gemLeftPosition);
        if (gemLeft != null && !gemLeft.GetComponent<GemBehaviour>().usedForRootSearch)
        {
            if (Search(gemLeftPosition, ref subRootPath))
            {
                rootPath += subRootPath;
                hasRootPath = true;
                return;
            }
        }
        Vector2Int gemRightPosition = new Vector2Int(positionInList.x + 1, positionInList.y);
        GameObject gemRight = manager.GetGem(gemRightPosition);
        if (gemRight != null && !gemRight.GetComponent<GemBehaviour>().usedForRootSearch)
        {
            if (Search(gemRightPosition, ref subRootPath))
            {
                rootPath += subRootPath;
                hasRootPath = true;
                return;
            }
        }
    }

    private bool Search (Vector2Int fromPos, ref string subRootPath)
    {
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