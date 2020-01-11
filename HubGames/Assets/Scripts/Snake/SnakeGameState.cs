using System;
using UnityEngine;

public class SnakeGameState : MonoBehaviour
{
    public static bool GameOver { get; private set; } = true;

    private AudioSource audioSource;

    private void Awake ()
    {
        ResourceManager.AddResource<AudioClip>("snakeEat", "Snake/snake_eat", HubGames.SNAKE);
        ResourceManager.AddResource<AudioClip>("snakeHit", "Snake/snakeHit", HubGames.SNAKE);
    }

    private void Start ()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        InputSystem.Instance.OnGameRestartInput += OnSnakeRestart;
        SnakeGrid.OnGridCollision += OnGameOver;
    }

    private void OnDestroy ()
    {
        InputSystem.Instance.OnGameRestartInput -= OnSnakeRestart;
        SnakeGrid.OnGridCollision -= OnGameOver;
        GameOver = true;
    }

    private void OnGameOver ()
    {
        audioSource.PlayOneShot(ResourceManager.GetResource<AudioClip>("snakeHit"));
        GameOver = true;
    }

    private void OnSnakeRestart ()
    {
        GameOver = false;
    }
}