using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class HubUI : MonoBehaviour
{
    [SerializeField] private GameObject[] hubButtons;

    // Start is called before the first frame update
    private void Start ()
    {
        foreach (GameObject button in hubButtons)
        {
            StartCoroutine(PlayVideo(
                button.GetComponent<VideoPlayer>(),
                button.GetComponent<RawImage>()));
        }
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