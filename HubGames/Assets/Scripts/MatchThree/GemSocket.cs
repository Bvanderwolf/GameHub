using UnityEngine;

public struct GemSocket
{
    public GameObject gem;
    public Vector2 pos;

    public GemSocket (GameObject _gem, Vector2 _pos)
    {
        gem = _gem;
        pos = _pos;
    }
}