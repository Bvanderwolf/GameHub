using UnityEngine;

public class MatchThreePlayerController : MonoBehaviour
{
    [SerializeField] private GameObject pivot;
    [SerializeField] private GameObject indicator;
    [SerializeField] private GameObject gemPrefab;

    [SerializeField] private SpriteRenderer gemHolderRend;

    [SerializeField] private float rotateSpeed;
    [SerializeField] private float shootForce;

    private const float SHOOTDELAY = 0.5f;
    private const float MAXROTATION = 70;
    private readonly Vector3 maxPosition = new Vector3(0.9425f, 0.6658f, 0);

    private Timer shootTimer;
    private bool canShoot = true;

    private void Start ()
    {
        gemHolderRend.sprite = GemManager.GetRandomGemSprite();
    }

    private void Update ()
    {
        if (MatchThreeGameState.GameOver)
            return;

        if (shootTimer != null)
            shootTimer.Tick(Time.deltaTime);

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            indicator.transform.RotateAround(pivot.transform.position, Vector3.forward, rotateSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            indicator.transform.RotateAround(pivot.transform.position, Vector3.forward, -(rotateSpeed * Time.deltaTime));
        }

        if (Input.GetKeyDown(KeyCode.Space) && canShoot)
        {
            GameObject gem = Instantiate(gemPrefab, pivot.transform.position, Quaternion.identity);
            gem.GetComponent<SpriteRenderer>().sprite = gemHolderRend.sprite;
            gem.GetComponent<Rigidbody2D>().AddForce(indicator.transform.up * shootForce);
            gemHolderRend.sprite = GemManager.GetRandomGemSprite();

            canShoot = false;
            if (shootTimer != null)
            {
                shootTimer.Reset();
            }
            else
            {
                shootTimer = new Timer(SHOOTDELAY, () => canShoot = true);
            }
        }

        //Debug.DrawRay(indicator.transform.position, indicator.transform.up, Color.red);

        Vector3 euler = indicator.transform.localEulerAngles;
        euler.z = ClampAngle(euler.z, -MAXROTATION, MAXROTATION);
        indicator.transform.localEulerAngles = euler;

        indicator.transform.localPosition = ClampedPosition();
    }

    private float ClampAngle (float angle, float from, float to)
    {
        // accepts e.g. -80, 80
        if (angle < 0f) angle = 360 + angle;
        if (angle > 180f) return Mathf.Max(angle, 360 + from);
        return Mathf.Min(angle, to);
    }

    private Vector3 ClampedPosition ()
    {
        Vector3 pos = indicator.transform.localPosition;
        int clampDimensions = 3;
        for (int i = 0; i < clampDimensions; i++)
        {
            pos[i] = Mathf.Clamp(pos[i], -maxPosition[i], maxPosition[i]);
        }
        return pos;
    }
}