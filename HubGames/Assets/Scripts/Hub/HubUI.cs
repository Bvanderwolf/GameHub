using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class HubUI : MonoBehaviour
{
    [SerializeField] private GameObject[] hubButtons;

    private readonly float BUTTON_WIDTH_HEIGHT_RELATION = 0.75f;
    private readonly float BUTTON_X_Y_RELATION = 0.9f;

    private readonly float CAMERA_TO_BUTTON_WIDTH_RATIO = 0.4f;
    private readonly float CAMERA_TO_BUTTON_POS_RATIO = 0.25f;

    private void Start ()
    {
        foreach (GameObject button in hubButtons)
        {
            RectTransform rectTF = button.GetComponent<RectTransform>();
            SetWidthHeight(rectTF, Camera.main.pixelWidth);
            SetPosition(rectTF, Camera.main.pixelWidth);
            StartCoroutine(PlayVideo(
                button.GetComponent<VideoPlayer>(),
                button.GetComponent<RawImage>()));
        }
    }

    private void SetPosition (RectTransform rectTF, float cameraWidth)
    {
        float x = cameraWidth * CAMERA_TO_BUTTON_POS_RATIO;
        float y = rectTF.anchoredPosition.y < 0 ? -x * BUTTON_X_Y_RELATION : x * BUTTON_X_Y_RELATION;
        rectTF.anchoredPosition = new Vector2(rectTF.anchoredPosition.x < 0 ? -x : x, y);
    }

    private void SetWidthHeight (RectTransform rectTF, float cameraWidth)
    {
        float width = cameraWidth * CAMERA_TO_BUTTON_WIDTH_RATIO;
        float height = rectTF.sizeDelta.y < 0 ? -width * BUTTON_WIDTH_HEIGHT_RELATION : width * BUTTON_WIDTH_HEIGHT_RELATION;
        rectTF.sizeDelta = new Vector2(rectTF.sizeDelta.x < 0 ? -width : width, height);
    }

    private IEnumerator PlayVideo (VideoPlayer videoPlayer, RawImage image)
    {
        videoPlayer.Prepare();
        yield return new WaitUntil(() => videoPlayer.isPrepared);
        image.texture = videoPlayer.texture;
        videoPlayer.Play();

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