using System.Collections.Generic;
using UnityEngine;
using System;

public class GemBehaviour : MonoBehaviour
{
    private GemManager manager;
    private SpriteRenderer spriteRend;
    [SerializeField] private Vector2Int positionInList;

    public bool Explodable = false;

    public Vector2Int PositionInList => positionInList;

    public event Action OnTriedExploding;

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

                TryExplosion();
                OnTriedExploding();
            }
        }
    }

    private void TryExplosion ()
    {
        Explodable = true;
        List<Vector2Int> positions = manager.GetPositionsOfGemsWithSprite(spriteRend.sprite);

        positions.ForEach((pos) =>
        {
            if (manager.IsNeighbour(positionInList, pos))
            {
                GemBehaviour behaviour = manager.GetGem(pos)?.GetComponent<GemBehaviour>();
                if (behaviour != null && !behaviour.Explodable)
                {
                    //Debug.Log($"found {behaviour} @ {pos} from {positionInList}");
                    behaviour.TryExplosion();
                }
            }
        });
    }
}