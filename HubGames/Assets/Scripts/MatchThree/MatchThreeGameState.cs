using System;
using UnityEngine;

public class MatchThreeGameState : MonoBehaviour
{
    public static bool GameOver { get; private set; } = true;

    public event Action OnGameOver;

    private void Awake ()
    {
        ResourceManager.AddResource<Sprite>("gemBlue", "MatchThree/gemBlue", HubGames.MATCHTHREE);
        ResourceManager.AddResource<Sprite>("gemGreen", "MatchThree/gemGreen", HubGames.MATCHTHREE);
        ResourceManager.AddResource<Sprite>("gemRed", "MatchThree/gemRed", HubGames.MATCHTHREE);
        ResourceManager.AddResource<Sprite>("gemYellow", "MatchThree/gemYellow", HubGames.MATCHTHREE);
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
    }

    private void OnRestart ()
    {
        GameOver = false;
    }

    private void OnLoseEndState ()
    {
        Debug.Log("Lost game!");
        GameOver = true;
        OnGameOver();
    }

    private void OnWinEndState ()
    {
        Debug.Log("won game!");
        GameOver = true;
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