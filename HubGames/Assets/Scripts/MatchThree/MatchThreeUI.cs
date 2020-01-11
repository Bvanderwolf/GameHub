using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchThreeUI : MonoBehaviour
{
    [SerializeField] private GameObject dotPrefab;

    private const float DOT_MARGIN = 5f;
    private Vector2 startOffset;

    private float dotWidth;
    private float dotHeight;

    private Texture redDot;
    private Texture greenDot;
    private List<GameObject> dots = new List<GameObject>();

    private bool showing = true;

    private void Awake ()
    {
        redDot = ResourceManager.GetResource<Texture>("dotRed");
        greenDot = ResourceManager.GetResource<Texture>("dotGreen");

        dotWidth = dotPrefab.GetComponent<RectTransform>().rect.width;
        dotHeight = dotPrefab.GetComponent<RectTransform>().rect.height;
    }

    public void UpdateVisual (GemManager manager)
    {
        dots.Clear();

        float totalWidthHalf = dotWidth * manager.MAX_START_GEMS_PER_ROW * 0.5f;
        float totalHeightHalf = dotHeight * manager.Rows * 0.5f;

        startOffset = new Vector2(totalWidthHalf + dotWidth, Camera.main.pixelHeight - totalHeightHalf - (dotHeight * 2));

        /*to center the gems we use negative half of the total width and
        height as our x and y and to make it appear on top of the screen we offset y*/
        Func<bool, Vector2> StartPosition = (_isEvenRow) =>
        {
            float x = _isEvenRow ? -totalWidthHalf : -totalWidthHalf + (dotWidth * 0.6f);

            return new Vector2(
            x + startOffset.x, -totalHeightHalf + startOffset.y + ((manager.Rows - 1) * dotHeight) + ((manager.Rows - 1) * DOT_MARGIN));
        };

        for (int row = 0; row < manager.Gems.Count; row++)
        {
            bool isEven = manager.Gems[row].Count == manager.MAX_START_GEMS_PER_ROW;
            for (int col = 0; col < manager.Gems[row].Count; col++)
            {
                GameObject dot = Instantiate(
                    dotPrefab,
                    StartPosition(isEven) + new Vector2(
                        (col * dotWidth) + (col * DOT_MARGIN),
                        -(row * dotHeight) - (row * DOT_MARGIN)),
                    Quaternion.identity,
                    transform);
                dot.GetComponent<RawImage>().texture = manager.Gems[row][col] != null ? greenDot : redDot;
                dots.Add(dot);
            }
        }
    }

    [ContextMenu("ShowOrHideVisuals")]
    public void ShowOrHideVisuals ()
    {
        showing = !showing;

        foreach (GameObject dot in dots)
        {
            dot.SetActive(showing);
        }
    }
}