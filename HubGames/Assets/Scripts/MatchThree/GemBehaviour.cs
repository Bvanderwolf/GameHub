using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Collections;

public class GemBehaviour : MonoBehaviour
{
    private GemManager manager;
    private SpriteRenderer spriteRend;
    [SerializeField] private Vector2Int positionInList;

    public Vector2Int PositionInList => positionInList;

    private readonly float explodeDelay = 0.1f;
    private readonly float fadeSpeed = 1.5f;

    private void Awake ()
    {
        spriteRend = GetComponent<SpriteRenderer>();
    }

    public void initialize (GemManager _manager, Vector2Int _position)
    {
        manager = _manager;
        positionInList = _position;
    }

    public void Recruit (GameObject recruitedGem)
    {
        if (manager != null)
        {
            recruitedGem.GetComponent<GemBehaviour>().initialize(
                manager,
                manager.RecruitGem(recruitedGem, this.gameObject));
        }
    }

    private void OnCollisionEnter2D (Collision2D collision)
    {
        if (collision.gameObject.tag == "Gem")
        {
            Rigidbody2D body = GetComponent<Rigidbody2D>();
            if (!body.isKinematic)
            {
                body.velocity = Vector2.zero;
                body.isKinematic = true;

                collision.gameObject.GetComponent<GemBehaviour>().Recruit(this.gameObject);
            }
        }
    }

    public void RemoveSelf (int positionInExplodingGems)
    {
        StartCoroutine(FadeToDestroy(positionInExplodingGems * explodeDelay));
    }

    private IEnumerator FadeToDestroy (float delay)
    {
        yield return new WaitForSeconds(delay);

        float currentLerpTime = 0;
        while (currentLerpTime != 1)
        {
            currentLerpTime += Time.deltaTime * fadeSpeed;
            if (currentLerpTime > 1) currentLerpTime = 1;

            float lerpedAlpha = 1 + (currentLerpTime * (0 - 1));
            spriteRend.color = new Color(1, 1, 1, lerpedAlpha);
            yield return null;
        }
        Destroy(this.gameObject);
    }
}