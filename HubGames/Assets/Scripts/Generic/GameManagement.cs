using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum HubGames { NONE, TICTACTOE, SNAKE, BREAKOUT, MATCHTHREE }

public class GameManagement : MonoBehaviour
{
    public static GameManagement Instance { get; private set; }

    public event Action OnLoadHubStart;

    private Dictionary<HubGames, int> hubDict = new Dictionary<HubGames, int>();

    private HubGames hubGamePlaying = HubGames.NONE;

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

        //set screen settings for small screen
        SetScreenSetings();

        DontDestroyOnLoad(this.gameObject);
    }

    private void Start ()
    {
        if (Instance == this)
        {
            HubGames[] hubGameTypes = (HubGames[])Enum.GetValues(typeof(HubGames));
            for (int i = 0; i < hubGameTypes.Length; i++)
            {
                hubDict.Add(hubGameTypes[i], i);
            }
            ResourceManager.GiveKnownHubGames(hubGameTypes);
            InputSystem.Instance.OnButtonPress += OnButtonPress;
        }
    }

    public void TryEndHubGame ()
    {
        if (hubGamePlaying != HubGames.NONE)
        {
            OnLoadHubStart?.Invoke();
            SceneManager.LoadScene(hubDict[HubGames.NONE]);
        }
    }

    public bool HubGameOver ()
    {
        switch (hubGamePlaying)
        {
            case HubGames.NONE: return false;
            case HubGames.SNAKE: return SnakeGameState.GameOver;
            case HubGames.TICTACTOE: return TicTacToeGameState.GameOver;
            case HubGames.BREAKOUT: return BreakoutGameState.GameOver;
            case HubGames.MATCHTHREE: return MatchThreeGameState.GameOver;
            default: return false;
        }
    }

    private void OnButtonPress (HubGames toHubgame)
    {
        SceneManager.LoadScene(hubDict[toHubgame]);
        hubGamePlaying = toHubgame;
    }

    private void SetScreenSetings ()
    {
        if (Screen.fullScreen)
        {
            PlayerPrefs.SetInt("Screenmanager Resolution Width", 500);
            PlayerPrefs.SetInt("Screenmanager Resolution Height", 500);
            PlayerPrefs.SetInt("Screenmanager Is Fullscreen mode", 0);
        }
    }
}