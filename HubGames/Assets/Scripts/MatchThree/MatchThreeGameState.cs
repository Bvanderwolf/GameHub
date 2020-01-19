using System;
using UnityEngine;

public class MatchThreeGameState : MonoBehaviour
{
    public static bool GameOver { get; private set; } = true;

    public event Action OnGameOver;

    private AudioClip endingSound;
    private AudioSource audioSource;

    private void Awake ()
    {
        ResourceManager.AddResource<Sprite>("gemBlue", "MatchThree/gemBlue", HubGames.MATCHTHREE);
        ResourceManager.AddResource<Sprite>("gemGreen", "MatchThree/gemGreen", HubGames.MATCHTHREE);
        ResourceManager.AddResource<Sprite>("gemRed", "MatchThree/gemRed", HubGames.MATCHTHREE);
        ResourceManager.AddResource<Sprite>("gemYellow", "MatchThree/gemYellow", HubGames.MATCHTHREE);
        //for match three we use the same audioclips as for breakout
        ResourceManager.AddResource<AudioClip>("breakoutEnding", "Breakout/breakoutEnd", HubGames.BREAKOUT);
        ResourceManager.AddResource<AudioClip>("ballHit", "Breakout/ball_hit", HubGames.BREAKOUT);

        audioSource = GetComponent<AudioSource>();
    }

    private void Start ()
    {
        GemManager gemManager = FindObjectOfType<GemManager>();
        gemManager.OnAllGemsDestroyed += OnWinEndState;
        gemManager.OnMaxRowsReached += OnLoseEndState;

        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.OnGameRestartInput += OnRestart;
        }
        endingSound = ResourceManager.GetResource<AudioClip>("breakoutEnding");
    }

    private void OnRestart ()
    {
        GameOver = false;
    }

    private void OnLoseEndState ()
    {
        Debug.Log("Lost game!");
        SetGameOver();
    }

    private void OnWinEndState ()
    {
        Debug.Log("won game!");
        SetGameOver();
    }

    private void SetGameOver ()
    {
        GameOver = true;
        audioSource.PlayOneShot(endingSound);
        OnGameOver();
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