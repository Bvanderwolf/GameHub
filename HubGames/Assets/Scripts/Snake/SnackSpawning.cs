using UnityEngine;
using UnityEngine.UI;

public class SnackSpawning : MonoBehaviour
{
    private readonly Color standardColor = new Color(1, 0, 0);
    private readonly Color specialColor = new Color(0, 0, 1);
    private readonly Color timedColor = new Color(0, 1, 0);

    public enum SnackType { STANDARD, SPECIAL, TIMED }

    private int spawnCount = 0;

    private bool SpecialSpawnable => Random.Range(0, 1f) < 0.15f || spawnCount % 6 == 0;
    private bool TimedSpawnable => Random.Range(0, 1f) < 0.1f || spawnCount % 9 == 0;

    public GameObject SpawnSnack (GameObject prefab, Vector2 position, out SnackType type)
    {
        spawnCount++;
        GameObject snack = Instantiate(prefab, position, Quaternion.identity, transform);
        type = GetSnackType();
        SetSnackColor(snack, type);
        return snack;
    }

    private SnackType GetSnackType ()
    {
        if (SpecialSpawnable)
        {
            return SnackType.SPECIAL;
        }
        if (TimedSpawnable)
        {
            return SnackType.TIMED;
        }
        return SnackType.STANDARD;
    }

    private void SetSnackColor (GameObject snack, SnackType type)
    {
        RawImage image = snack.GetComponent<RawImage>();
        switch (type)
        {
            case SnackType.STANDARD: image.color = standardColor; break;
            case SnackType.SPECIAL: image.color = specialColor; break;
            case SnackType.TIMED: image.color = timedColor; break;
        }
    }

    public void ResetTimedSnack (GameObject snack)
    {
        snack.GetComponent<RawImage>().color = standardColor;
    }
}