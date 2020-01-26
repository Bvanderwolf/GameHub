using UnityEngine;

public class BreakoutUI : MonoBehaviour
{
    [SerializeField] private GameObject startText;

    private void Start ()
    {
        if (InputSystem.Instance != null)
        {
            InputSystem.Instance.OnGameRestartInput += OnRestart;
        }
        FindObjectOfType<BreakoutGameState>().OnGameOver += OnGameOver;
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