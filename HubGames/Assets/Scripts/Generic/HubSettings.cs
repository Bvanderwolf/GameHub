using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HubSettings : MonoBehaviour
{
    public static HubSettings Instance { get; private set; }

    public readonly float defaultScreenWidthRatio = 0.002f;
    public readonly float defaultScreenHeightRatio = 0.002f;

    public Vector2 ScreenRatio { get; private set; }

    public event Action OnScreenChanged;

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
        GameManagement.Instance.OnLoadHubStart += OnLoadHubStart;
        UISystem.Instance.OnUIReInitialized += ConnectWithHubUI;
        ConnectWithHubUI();
        SetScreenSettings();
    }

    private void OnLoadHubStart ()
    {
        SceneManager.sceneLoaded += OnHubLoaded;
    }

    private void OnHubLoaded (Scene scene, LoadSceneMode mode)
    {
        ConnectWithHubUI();
        SceneManager.sceneLoaded -= OnHubLoaded;
    }

    private void ConnectWithHubUI ()
    {
        FindObjectOfType<HubUI>().OnResolutionButtonClicked += SetResolutionSettingsFromButton;
    }

    private void SetResolutionSettingsFromButton (string name)
    {
        string[] widthHeight = name.Split('x');
        int width = int.Parse(widthHeight[0]);
        int height = int.Parse(widthHeight[1]);
        if (!(width == Screen.width && height == Screen.height))
        {
            Screen.SetResolution(width, height, PlayerPrefs.GetInt("Screenmanager Is Fullscreen mode") == 1 ? true : false);

            PlayerPrefs.SetInt("Screenmanager Resolution Width", width);
            PlayerPrefs.SetInt("Screenmanager Resolution Height", height);

            StartCoroutine(WaitForScreenChange(width));
        }
    }

    private IEnumerator WaitForScreenChange (int width)
    {
        while (width != Screen.width) yield return null;

        SetScreenSettings();
    }

    private void SetScreenSettings ()
    {
        ScreenRatio = new Vector2(Screen.width * defaultScreenWidthRatio, Screen.height * defaultScreenHeightRatio);
        Rect pixelRect = Camera.main.pixelRect;
        Camera.main.pixelRect = new Rect(pixelRect.x, pixelRect.y, Screen.width, Screen.height);
        OnScreenChanged?.Invoke();
    }
}