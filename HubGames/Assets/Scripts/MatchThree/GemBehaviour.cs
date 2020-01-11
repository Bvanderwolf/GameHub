using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GemBehaviour : MonoBehaviour
{
    private GemManager manager;
    private SpriteRenderer spriteRend;

    public bool Chained = false;
    public bool isRoot;

    private void Awake ()
    {
        spriteRend = GetComponent<SpriteRenderer>();
    }

    public void initialize (GemManager _manager)
    {
        manager = _manager;
        isRoot = manager.IsRootGem(this.gameObject);
    }

    public void Recruit (GameObject recruitedGem)
    {
        if (manager != null)
        {
            recruitedGem.GetComponent<GemBehaviour>().initialize(manager);
            manager.RecruitGem(recruitedGem, this.gameObject);
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
}