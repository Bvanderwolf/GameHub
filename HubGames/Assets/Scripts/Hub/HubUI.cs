using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class HubUI : MonoBehaviour
{
    [SerializeField] private GameObject[] gameButtons;
    [SerializeField] private GameObject[] resolutionButtons;
    [SerializeField] private GameObject resolutionText;

    private readonly float BUTTON_WIDTH_HEIGHT_RELATION = 0.75f;
    private readonly float BUTTON_X_Y_RELATION = 0.9f;

    private readonly float CAMERA_TO_BUTTON_WIDTH_RATIO = 0.4f;
    private readonly float CAMERA_TO_BUTTON_POS_RATIO = 0.25f;

    private readonly float RESOLUTIONBUTTON_MARGIN = 20;
    private readonly float RESOLUTIONBUTTON_SLIDE_SPEED = 2.5f;
    private float resolutionButtonXOffset;

    private bool movingResolutionButtons = false;

    public event Action<string> OnResolutionButtonClicked;

    private void Start ()
    {
        SetGameButtonAttributes();
        SetResolutionButtonAttributes();
    }

    private void SetGameButtonAttributes ()
    {
        foreach (GameObject button in gameButtons)
        {
            RectTransform rectTF = button.GetComponent<RectTransform>();
            SetGameButtonWidthHeight(rectTF, Camera.main.pixelWidth);
            SetGameButtonPosition(rectTF, Camera.main.pixelWidth);
            VideoPlayer videoPlayer = button.GetComponent<VideoPlayer>();

            if (!videoPlayer.isPlaying)
            {
                StartCoroutine(PlayVideo(
                button.GetComponent<VideoPlayer>(),
                button.GetComponent<RawImage>()));
            }
        }
    }

    private void SetResolutionButtonAttributes ()
    {
        float resolutionButtonWidth = resolutionButtons[0].GetComponent<RectTransform>().sizeDelta.x;
        resolutionButtonXOffset = (resolutionButtonWidth + RESOLUTIONBUTTON_MARGIN) * HubSettings.Instance.ScreenRatio.x;
        foreach (GameObject button in resolutionButtons)
        {
            RectTransform rectTF = button.GetComponent<RectTransform>();
            float sizeX = rectTF.sizeDelta.x * HubSettings.Instance.ScreenRatio.x;
            float sizeY = rectTF.sizeDelta.y * HubSettings.Instance.ScreenRatio.y;
            rectTF.sizeDelta = new Vector2(sizeX, sizeY);

            GameObject text = rectTF.GetChild(0).gameObject;
            float textScale = text.GetComponent<RectTransform>().localScale.x * HubSettings.Instance.ScreenRatio.x;
            ScaleText(text, textScale);
        }
        RectTransform rectTFTitleText = resolutionText.GetComponent<RectTransform>();
        float titleYPos = rectTFTitleText.anchoredPosition.y * HubSettings.Instance.ScreenRatio.y;
        float titleScale = rectTFTitleText.localScale.x * HubSettings.Instance.ScreenRatio.x;
        rectTFTitleText.anchoredPosition = new Vector2(rectTFTitleText.anchoredPosition.x, titleYPos);
        ScaleText(resolutionText, titleScale);
    }

    private void ScaleText (GameObject text, float scale)
    {
        RectTransform rectTF = text.GetComponent<RectTransform>();
        rectTF.sizeDelta = new Vector2(rectTF.sizeDelta.x * scale, rectTF.sizeDelta.y * scale);
        Text textComp = text.GetComponent<Text>();
        textComp.fontSize = (int)(textComp.fontSize * scale);
    }

    public void ShowOrHideResolutionButtons ()
    {
        movingResolutionButtons = true;
        resolutionText.SetActive(!resolutionText.activeInHierarchy);
        foreach (GameObject button in resolutionButtons)
        {
            StartCoroutine(ShowOrHideResolutionButton(button));
        }
    }

    public void OnResolutionButtonClick (string name)
    {
        OnResolutionButtonClicked(name);
    }

    private void SetGameButtonPosition (RectTransform rectTF, float cameraWidth)
    {
        float x = rectTF.anchoredPosition.x * HubSettings.Instance.ScreenRatio.x;
        float y = rectTF.anchoredPosition.y * HubSettings.Instance.ScreenRatio.y;
        rectTF.anchoredPosition = new Vector2(x, y);

        RectTransform rectTFText = rectTF.GetChild(0).GetComponent<RectTransform>();
        float yText = rectTFText.anchoredPosition.y * HubSettings.Instance.ScreenRatio.y;
        rectTFText.anchoredPosition = new Vector2(rectTFText.anchoredPosition.x, yText);
    }

    private void SetGameButtonWidthHeight (RectTransform rectTF, float cameraWidth)
    {
        float x = rectTF.sizeDelta.x * HubSettings.Instance.ScreenRatio.x;
        float y = rectTF.sizeDelta.y * HubSettings.Instance.ScreenRatio.y;
        rectTF.sizeDelta = new Vector2(x, y);

        GameObject text = rectTF.GetChild(0).gameObject;
        float scale = text.GetComponent<RectTransform>().localScale.x * HubSettings.Instance.ScreenRatio.x;
        ScaleText(text, scale);
    }

    private IEnumerator ShowOrHideResolutionButton (GameObject button)
    {
        //store whether the button needs to be shown and if so show the button
        bool showing = !button.activeInHierarchy;
        if (showing) button.SetActive(true);

        //the button in the middle doesn't need to move
        if (button.transform.localPosition.x == 0)
        {
            if (!showing) button.SetActive(false);
            yield break;
        }

        //setup lerping values
        float currentLerpTime = 0;
        float xTarget = button.transform.localPosition.x < 0 ? -1 : 1;
        float xStart = button.transform.localPosition.x;
        if (showing)
            xTarget = button.transform.localPosition.x < 0 ? -resolutionButtonXOffset : resolutionButtonXOffset;

        //move button by linear interpolation between start and target values
        while (currentLerpTime != 1f)
        {
            currentLerpTime += Time.deltaTime * RESOLUTIONBUTTON_SLIDE_SPEED;
            if (currentLerpTime > 1f)
                currentLerpTime = 1f;

            button.transform.localPosition = new Vector2(xStart + (currentLerpTime * (xTarget - xStart)), 0);
            yield return null;
        }
        //if the goal was to hide the button and it is active we deactivate it
        if (!showing && button.activeInHierarchy) button.SetActive(false);
    }

    private IEnumerator PlayVideo (VideoPlayer videoPlayer, RawImage image)
    {
        //prepare the video player and wait till it is ready
        videoPlayer.Prepare();
        yield return new WaitUntil(() => videoPlayer.isPrepared);

        //set image texture as target texture to display on
        image.texture = videoPlayer.texture;
        videoPlayer.Play();

        //fade in video player by linearly interpolating between 0 and 1 alpha values
        float currentLerpTime = 0;
        while (currentLerpTime != 1f)
        {
            currentLerpTime += Time.deltaTime;
            if (currentLerpTime > 1f) currentLerpTime = 1f;
            image.color = new Color(1, 1, 1, currentLerpTime * 1);
            yield return null;
        }
    }

    private void OnDestroy ()
    {
        StopAllCoroutines();
    }
}