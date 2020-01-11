using UnityEngine;
using UnityEngine.UI;

public class TicTacToeCanvas : MonoBehaviour
{
    public static bool PoppingUpPlayerPlayingText { get; private set; }

    [SerializeField] private GameObject playerPlayingText;
    [SerializeField] private GameObject gameOverText;

    private Color playerPlayingTextColor;

    private void Awake ()
    {
        playerPlayingTextColor = playerPlayingText.GetComponent<Text>().color;
    }

    private void Start ()
    {
        UISystem.Instance.OnUISystemRestartEvent += OnRestart;
        TicTacToeGrid.OnGameOver += OnGameOver;
        TicTacToeGameState.OnTurnEnd += StartPlayerPlayingTextPopup;
    }

    private void OnDestroy ()
    {
        UISystem.Instance.OnUISystemRestartEvent -= OnRestart;
        TicTacToeGrid.OnGameOver -= OnGameOver;
        TicTacToeGameState.OnTurnEnd -= StartPlayerPlayingTextPopup;
    }

    private void StartPlayerPlayingTextPopup ()
    {
        PoppingUpPlayerPlayingText = true;
        UISystem.Instance.PopupTextWithFade(
            playerPlayingText,
            $"Player {TicTacToeGameState.NumOfPlayerPlaying} Turn",
            OnPlayerPlayingTextPopupEnd);
    }

    private void OnPlayerPlayingTextPopupEnd ()
    {
        playerPlayingText.transform.localScale = Vector3.zero;
        playerPlayingText.GetComponent<Text>().color = playerPlayingTextColor;
        PoppingUpPlayerPlayingText = false;
    }

    private void OnRestart ()
    {
        gameOverText.SetActive(false);
    }

    private void OnGameOver ()
    {
        //base text to be shown on num of player winning
        int numOfWinner = TicTacToeGameState.NumOfPlayerPlaying;
        string gameOverString;
        if (numOfWinner == 1 || numOfWinner == 2)
        {
            gameOverString = $"Player {numOfWinner} won";
        }
        else if (numOfWinner == 0)
        {
            gameOverString = "Game ended without winner";
        }
        else
        {
            gameOverString = $"Error ocurred cause numOfWinner: {numOfWinner}";
        }
        gameOverString += "\nPress Enter to retry";
        gameOverText.GetComponent<Text>().text = gameOverString;
        gameOverText.SetActive(true);
    }
}