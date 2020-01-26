using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BallBehaviour : MonoBehaviour
{
    [SerializeField] private float startForce;

    private Vector2 direction;

    public event Action OnBottomBoundHit;

    private AudioClip hitSound;
    private AudioSource audioSource;

    private void Awake ()
    {
        PushBallInRandomDirection();
    }

    private void PushBallInRandomDirection ()
    {
        while (direction.y < 0.2f)
        {
            direction = Random.insideUnitCircle;
        }

        GetComponent<Rigidbody2D>().AddForce(direction * startForce);
    }

    private void Start ()
    {
        audioSource = GetComponent<AudioSource>();
        hitSound = ResourceManager.GetResource<AudioClip>("ballHit");
    }

    private void OnCollisionEnter2D (Collision2D collision)
    {
        if (collision.gameObject.name == "Bottom")
        {
            OnBottomBoundHit?.Invoke();
            Destroy(this.gameObject);
        }

        if (collision.gameObject.tag == "Brick")
        {
            collision.gameObject.GetComponent<BrickAttributes>().TakeDamage();
        }
        audioSource.PlayOneShot(hitSound);
    }
}