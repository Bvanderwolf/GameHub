using System;
using UnityEngine;

public class SnakeUI : MonoBehaviour
{
    [SerializeField] private GameObject countdownTimer;
    [SerializeField] private GameObject startGameText;
    [SerializeField] private int countdownCount;

    public event Action OnCountDownEnd;

    private void Start ()
    {
        InputSystem.Instance.OnGameRestartInput += OnRestart;
        FindObjectOfType<SnakeGrid>().OnGridCollision += OnGameOver;

        float startGameTextScale = startGameText.transform.localScale.x * HubSettings.Instance.ScreenRatio.x;
        UISystem.Instance.ScaleText(startGameText, startGameTextScale);
    }

    private void OnDestroy ()
    {
        InputSystem.Instance.OnGameRestartInput -= OnRestart;
    }

    private void OnGameOver ()
    {
        startGameText.SetActive(true);
    }

    private void OnRestart ()
    {
        startGameText.SetActive(false);
        StartCountDown();
    }

    private void OnCountdownEnd ()
    {
        OnCountDownEnd?.Invoke();
    }

    private void StartCountDown ()
    {
        UISystem.Instance.CountDown(countdownTimer, countdownCount, true, OnCountdownEnd);
    }
}