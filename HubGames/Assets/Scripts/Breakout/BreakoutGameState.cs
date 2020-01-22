using System;
using UnityEngine;

public class BreakoutGameState : MonoBehaviour
{
    public static bool GameOver { get; private set; } = true;

    [SerializeField] private GameObject breakoutBallPrefab;

    private GameObject breakoutBall;

    public static event Action OnGameOver;

    private AudioClip endingSound;
    private AudioSource audioSource;

    private void Start ()
    {
        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.OnGameRestartInput += OnRestart;
        }
        audioSource = GetComponent<AudioSource>();
        endingSound = ResourceManager.GetResource<AudioClip>("breakoutEnding");
    }

    private void OnRestart ()
    {
        breakoutBall = Instantiate(breakoutBallPrefab);
        breakoutBall.GetComponent<BallBehaviour>().OnBottomBoundHit += OnBallBottomBoundHit;
        GameOver = false;
    }

    private void OnBallBottomBoundHit ()
    {
        OnGameOver?.Invoke();
        GameOver = true;
        audioSource.PlayOneShot(endingSound);
    }

    private void OnDestroy ()
    {
        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.OnGameRestartInput -= OnRestart;
        }
        GameOver = true;
    }
}