using UnityEngine;

public class BreakoutPlayerController : MonoBehaviour
{
    [SerializeField] private float force;

    private Vector3 startPostion;
    private Rigidbody2D rigidBody;

    private void Awake ()
    {
        startPostion = transform.position;
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void Start ()
    {
        FindObjectOfType<BreakoutGameState>().OnGameOver += OnGameOver;
    }

    private void Update ()
    {
        if (BreakoutGameState.GameOver)
            return;

        if (InputSystem.Instance.BreakoutPlayerLeftKey)
        {
            rigidBody.AddForce(Vector2.left * force);
        }
        if (InputSystem.Instance.BreakoutPlayerRightKey)
        {
            rigidBody.AddForce(Vector2.right * force);
        }
    }

    private void OnGameOver ()
    {
        transform.position = startPostion;
        rigidBody.velocity = Vector2.zero;
        rigidBody.angularVelocity = 0;
    }
}