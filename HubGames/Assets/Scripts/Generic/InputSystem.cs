using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class InputSystem : MonoBehaviour
{
    public static InputSystem Instance { get; private set; }

    public event Action<Vector3> OnCanvasClicked;

    public event Action OnGameRestartInput;

    public event Action<HubGames> OnButtonPress;

    public event Action<KeyCode> OnDirectionKeyDown;

    private List<KeyCode> directionKeys = new List<KeyCode>()
    {
        KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D,
        KeyCode.UpArrow, KeyCode.LeftArrow, KeyCode.DownArrow, KeyCode.RightArrow
    };

    public bool BreakoutPlayerLeftKey
    {
        get => Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
    }

    public bool BreakoutPlayerRightKey
    {
        get => Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
    }

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

        DontDestroyOnLoad(this.gameObject);
    }

    private void Start ()
    {
        if (Instance == this)
        {
            GameManagement.Instance.OnLoadHubStart += OnLoadHubStart;
            UISystem.Instance.OnUIReInitialized += HookUpLoadFunctionsFromUI;
        }
        HookUpLoadFunctionsFromUI();
    }

    private void OnLoadHubStart ()
    {
        SceneManager.sceneLoaded += OnHubLoaded;
    }

    private void OnHubLoaded (Scene scene, LoadSceneMode mode)
    {
        HookUpLoadFunctionsFromUI();
        SceneManager.sceneLoaded -= OnHubLoaded;
    }

    private void HookUpLoadFunctionsFromUI ()
    {
        foreach (Button btn in FindObjectsOfType(typeof(Button)))
        {
            switch (btn.gameObject.name)
            {
                case "BtnSnake": { btn.onClick.AddListener(OnStartSnakeButtonPress); break; }
                case "BtnTicTacToe": { btn.onClick.AddListener(OnTicTacToeButtonPress); break; }
                case "BtnBreakout": { btn.onClick.AddListener(OnBreakoutButtonPress); break; }
                case "BtnMatchThree": { btn.onClick.AddListener(OnMatchThreeButtonPress); break; }
                default: break;
            }
        }
    }

    private void OnMatchThreeButtonPress ()
    {
        OnButtonPress?.Invoke(HubGames.MATCHTHREE);
    }

    private void OnStartSnakeButtonPress ()
    {
        OnButtonPress?.Invoke(HubGames.SNAKE);
    }

    private void OnTicTacToeButtonPress ()
    {
        OnButtonPress?.Invoke(HubGames.TICTACTOE);
    }

    private void OnBreakoutButtonPress ()
    {
        OnButtonPress?.Invoke(HubGames.BREAKOUT);
    }

    // Update is called once per frame
    private void Update ()
    {
        //if the left mouse button is pressed we fire the oncanvasclicked event with mouse position
        if (Input.GetMouseButtonDown(0))
        {
            OnCanvasClicked?.Invoke(Input.mousePosition);
        }

        if (Input.GetKeyDown(KeyCode.Return) && GameManagement.Instance.HubGameOver())
        {
            OnGameRestartInput?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManagement.Instance.ResolveEscapePress();
        }

        for (int i = 0; i < directionKeys.Count; i++)
        {
            if (Input.GetKeyDown(directionKeys[i]))
            {
                OnDirectionKeyDown?.Invoke(directionKeys[i]);
                break;
            }
        }
    }
}