using System;
using UnityEngine;

public class TicTacToeGameState : MonoBehaviour
{
    public static int NumOfPlayerPlaying { get; private set; }
    public static bool GameOver { get; private set; } = true;

    public event Action OnTurnEnd;

    private readonly float startdelay = 1.25f;

    private AudioClip endingSound;
    private AudioSource audioSource;

    private Sprite playerOneSprite;
    private Sprite playerTwoSprite;

    private void Awake ()
    {
        //add player sprites
        ResourceManager.AddResource<Sprite>("playerOne", "TicTacToe/delete", HubGames.TICTACTOE);
        ResourceManager.AddResource<Sprite>("playerTwo", "TicTacToe/circle-outline", HubGames.TICTACTOE);
    }

    private void Start ()
    {
        playerOneSprite = ResourceManager.GetResource<Sprite>("playerOne");
        playerTwoSprite = ResourceManager.GetResource<Sprite>("playerTwo");
        endingSound = ResourceManager.GetResource<AudioClip>("gameEnd");

        audioSource = GetComponent<AudioSource>();

        InputSystem.Instance.OnGameRestartInput += OnRestart;
        FindObjectOfType<TicTacToeGrid>().OnGameOver += OnGameOver;
    }

    private void OnDestroy ()
    {
        InputSystem.Instance.OnGameRestartInput -= OnRestart;
        GameOver = true;
    }

    private void OnGameOver ()
    {
        GameOver = true;
        audioSource?.PlayOneShot(endingSound);
    }

    private void OnRestart ()
    {
        NumOfPlayerPlaying = 1;
        TimerManager.Instance.AddTimer("gamestart", new Timer(startdelay, () => OnTurnEnd?.Invoke()));
        GameOver = false;
    }

    /// <summary>
    /// switches numb of player playing
    /// </summary>
    public void SwitchTurns ()
    {
        NumOfPlayerPlaying = NumOfPlayerPlaying == 1 ? 2 : 1;
        if (!GameOver) OnTurnEnd?.Invoke();
    }

    public void SetNoWinner ()
    {
        NumOfPlayerPlaying = 0;
    }

    /// <summary>
    /// get player sprite based on turn
    /// </summary>
    /// <returns></returns>
    public Sprite GetPlayerSprite ()
    {
        return NumOfPlayerPlaying == 1 ? playerOneSprite : playerTwoSprite;
    }
}