using UnityEngine;
using System.Collections;
using System;
using System.Linq;

public class GemBehaviour : MonoBehaviour
{
    private GemManager manager;
    private SpriteRenderer spriteRend;
    [SerializeField] private Vector2Int positionInList;

    public Vector2Int PositionInList => positionInList;

    public event Action<GemBehaviour> OnRootSearchFailed;

    private readonly float explodeDelay = 0.1f;
    private readonly float explodeSpeed = 5f;
    private readonly float fadeSpeed = 1.5f;

    public bool exploding { get; private set; } = false;
    public bool exploded = false;
    public bool removingSelf { get; private set; } = false;

    [SerializeField] private bool usedForRootSearch = false;
    [SerializeField] private bool hasRootPath = false;

    private CircleCollider2D circleCollider;
    private Rigidbody2D body;
    private AudioClip hitSound;
    private AudioSource audioSource;

    private readonly float explodeRadius = 4.5f;
    private readonly float circleRadius = 2.6f;
    private readonly float fallYOffset = 1.5f;

    public string rootPath = "";

    private void Awake ()
    {
        spriteRend = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        body = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start ()
    {
        hitSound = ResourceManager.GetResource<AudioClip>("ballHit");
    }

    //sets up gem by getting manager reference, position, attaching OnRootSerachFailed event and creating root path
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

    private void OnCollisionEnter2D (Collision2D collision)
    {
        audioSource.PlayOneShot(hitSound);
    }

    private void OnTriggerEnter2D (Collider2D collision)
    {
        //If a Gem gets into contact with another gem it attaches if not kinematic
        if (collision.tag == "Gem")
        {
            if (!body.isKinematic) AttachToGem(collision);

            //if the gem is exploding when getting into contact with another gem,
            //it makes the other gem explode if it has the same sprite
            if (exploding)
            {
                //Debug.Log($"exploding at {positionInList}");
                if (collision.gameObject.GetComponent<SpriteRenderer>().sprite == spriteRend.sprite)
                {
                    collision.gameObject.GetComponent<GemBehaviour>().Explode();
                }
            }
        }
        else if (collision.tag == "Bound" && collision.name == "Top")
        {
            if (!body.isKinematic) AttachToTopBound();
        }
    }

    //Sets up physics values for getting recruited, starts exploding and lets itself be recruited by given colliding gem
    private void AttachToGem (Collider2D collision)
    {
        SetDefaultPhysicsValues();
        StartCoroutine(StartExplode());
        collision.gameObject.GetComponent<GemBehaviour>().Recruit(this.gameObject);
    }

    private void AttachToTopBound ()
    {
        GemManager manager = FindObjectOfType<GemManager>();
        if (manager != null)
        {
            SetDefaultPhysicsValues();
            StartCoroutine(StartExplode());
            manager.RecruitGemFromTopBound(this.gameObject);
        }
    }

    //Tries creating root path based on position in list.
    public void CreateRootPath ()
    {
        rootPath += $"{positionInList.x}:{positionInList.y}";
        //if the gem is already in the top row it already has it's root path created
        if (positionInList.y != 0)
        {
            SearchSequential();
            /*if the path created by sequential search is valid we reset values of gems searched through.
            If failing removes itself and fires OnRootSearchFailed event*/
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

    //tries verifying the current root path and tries creating a new one if it is not valid
    public void CheckOnRootPath ()
    {
        if (!VerifyRootPath())
        {
            rootPath = "";
            CreateRootPath();
        }
    }

    //goes through the root path string and resets usedForRootSearch values for searched through gems.
    //If no path is given the function will use the "rootPath" value
    private void ResetRootSearchValues (string path = "")
    {
        if (path == "") path = rootPath;

        string[] positions = path.Split(',');
        foreach (string stringPos in positions)
        {
            string[] axis = stringPos.Split(':');
            Vector2Int pos = new Vector2Int(int.Parse(axis[0]), int.Parse(axis[1]));
            GemBehaviour behaviour = manager.GetGem(pos)?.GetComponent<GemBehaviour>();
            if (behaviour.usedForRootSearch) behaviour.usedForRootSearch = false;
        }
    }

    //Tries verifying the root path string and returns is it valid or not
    private bool VerifyRootPath ()
    {
        //split rootpath up in seperate positions
        string[] positions = rootPath.Split(',');
        //if there are no positions with y = 0 meaning no root position we return false
        if (!positions.Any((s) => int.Parse(s.Split(':')[1]) == 0))
            return false;

        //if any of the positions in the list correlate to a non-existend gem we return false
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

    //goes through all possible searchable positionTypes to start from and 1 by 1 searches through them
    private void SearchSequential ()
    {
        string subRootPath = "";

        Func<Vector2Int, bool, Vector2Int>[] positionTypes = new Func<Vector2Int, bool, Vector2Int>[6]
        {
            (v, even) => new Vector2Int(v.x, v.y - 1), //top
            (v, even) => new Vector2Int(even ? v.x - 1 : v.x + 1, v.y - 1), //topSide based on even row
            (v, even) => new Vector2Int(v.x, v.y + 1), //bottom
            (v, even) => new Vector2Int(v.x + 1, v.y + 1), //bottom side
            (v, even) => new Vector2Int(v.x - 1, v.y), //left
            (v, even) => new Vector2Int(v.x + 1, v.y) //right
        };

        bool isEvenRow = (positionInList.y + 1) % 2 == 0;
        //go through each positionType and if found gem is not null and not used yet for root search
        //we use it to search from.
        for (int i = 0; i < positionTypes.Length; i++)
        {
            Vector2Int position = positionTypes[i](positionInList, isEvenRow);
            GemBehaviour behaviour = manager.GetGem(position)?.GetComponent<GemBehaviour>();
            if (behaviour != null && !behaviour.usedForRootSearch)
            {
                behaviour.usedForRootSearch = true;
                //if we find the root from a search we add the sub root path to our root path and break from the loop
                if (SearchFrom(position, ref subRootPath))
                {
                    rootPath += subRootPath;
                    hasRootPath = true;
                    break;
                }
                ResetRootSearchValues(subRootPath.Remove(0));
                subRootPath = "";
            }
        }
    }

    //searches from given position to neighbouring gems
    private bool SearchFrom (Vector2Int fromPos, ref string subRootPath)
    {
        subRootPath += $",{fromPos.x}:{fromPos.y}";
        //if the position reached has y = 0 it means we reached the root and the search is succesfully completed
        if (fromPos.y == 0) return true;

        Func<Vector2Int, bool, Vector2Int>[] positionTypes = new Func<Vector2Int, bool, Vector2Int>[6]
        {
            (v, even) => new Vector2Int(v.x, v.y - 1),
            (v, even) => new Vector2Int(even ? v.x - 1 : v.x + 1, v.y - 1),
            (v, even) => new Vector2Int(v.x, v.y + 1),
            (v, even) => new Vector2Int(v.x + 1, v.y + 1),
            (v, even) => new Vector2Int(v.x - 1, v.y),
            (v, even) => new Vector2Int(v.x + 1, v.y)
        };
        bool isEvenRow = (fromPos.y + 1) % 2 == 0;
        //go through each possitionType and if found gem is not null or already used for root search
        //we can search further from this gem
        for (int i = 0; i < positionTypes.Length; i++)
        {
            Vector2Int position = positionTypes[i](fromPos, isEvenRow);
            GemBehaviour behaviour = manager.GetGem(position)?.GetComponent<GemBehaviour>();
            if (behaviour != null && !behaviour.usedForRootSearch)
            {
                behaviour.usedForRootSearch = true;
                return SearchFrom(position, ref subRootPath);
            }
        }
        //if we all positionTypes have been iterated through without succes the search was unsuccesfull
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
        StartCoroutine(FadingFallToDestroy(positionInExplodingGems * explodeDelay));
    }

    //linearly interpolates alpha value of image color from 1 to 0 and then destroys gameobject
    private IEnumerator FadingFallToDestroy (float delay)
    {
        removingSelf = true;
        audioSource.PlayOneShot(hitSound);

        yield return new WaitForSeconds(delay);

        float currentLerpTime = 0;
        float startYPos = transform.position.y;
        float targetYPos = startYPos - fallYOffset;
        while (currentLerpTime != 1)
        {
            currentLerpTime += Time.deltaTime * fadeSpeed;
            if (currentLerpTime > 1) currentLerpTime = 1;

            float lerpedAlpha = 1 + (currentLerpTime * (0 - 1));
            spriteRend.color = new Color(1, 1, 1, lerpedAlpha);

            float yPosition = startYPos + (currentLerpTime * (targetYPos - startYPos));
            transform.position = new Vector2(transform.position.x, yPosition);
            yield return null;
        }
        manager.RemoveGemFromList(this);
        Destroy(this.gameObject);
    }
}