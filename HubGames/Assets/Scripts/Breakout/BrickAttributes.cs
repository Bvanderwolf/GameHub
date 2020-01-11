using UnityEngine;

public class BrickAttributes : MonoBehaviour
{
    [SerializeField] private int health = 1;
    private Color color;
    private Renderer rend;

    private void Awake ()
    {
        rend = GetComponent<Renderer>();
    }

    public void SetColor (Color _color)
    {
        color = _color;
        rend.material.color = _color;

        //for each 0 value, the brick's health increases with one
        for (int i = 0; i < 3; i++)
        {
            if (color[i] == 0)
            {
                health++;
            }
        }
    }

    [ContextMenu("takedamage")]
    public void TakeDamage ()
    {
        /*if the brick's health is not equal to one, we add a r, g or b value
        and subtract a health point. If it has only one health left we destroy it*/
        if (health != 1)
        {
            for (int i = 0; i < 3; i++)
            {
                if (color[i] == 0)
                {
                    color[i] = 255;
                    rend.material.color = color;
                    health--;
                    break;
                }
            }
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}