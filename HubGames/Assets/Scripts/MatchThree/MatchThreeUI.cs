using UnityEngine;

public class MatchThreeUI : MonoBehaviour
{
    [SerializeField] private GameObject startText;

    private void Start ()
    {
        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.OnGameRestartInput += OnRestart;
        }
        MatchThreeGameState gamestate = FindObjectOfType<MatchThreeGameState>();
        if (gamestate != null)
        {
            gamestate.OnGameOver += OnGameOver;
        }
    }

    private void OnRestart ()
    {
        startText.SetActive(false);
    }

    private void OnGameOver ()
    {
        startText.SetActive(true);
    }

    private void OnDestroy ()
    {
        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.OnGameRestartInput -= OnRestart;
        }
    }
}