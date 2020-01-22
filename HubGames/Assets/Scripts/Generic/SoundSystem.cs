using UnityEngine;

public class SoundSystem : MonoBehaviour
{
    public static SoundSystem Instance { get; private set; }

    private AudioSource audioSource;

    private void Awake ()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        audioSource = GetComponent<AudioSource>();

        AddAudioClipsToResourceManager();
        DontDestroyOnLoad(this.gameObject);
    }

    private void AddAudioClipsToResourceManager ()
    {
        ResourceManager.AddResource<AudioClip>("breakoutEnding", "Breakout/breakoutEnd", HubGames.BREAKOUT);
        ResourceManager.AddResource<AudioClip>("ballHit", "Breakout/ball_hit", HubGames.BREAKOUT);
        ResourceManager.AddResource<AudioClip>("snakeEat", "Snake/snake_eat", HubGames.SNAKE);
        ResourceManager.AddResource<AudioClip>("snakeHit", "Snake/snakeHit", HubGames.SNAKE);
        ResourceManager.AddResource<AudioClip>("place", "TicTacToe/tapv1", HubGames.TICTACTOE);
        ResourceManager.AddResource<AudioClip>("gameEnd", "TicTacToe/endingmusic", HubGames.TICTACTOE);
    }
}