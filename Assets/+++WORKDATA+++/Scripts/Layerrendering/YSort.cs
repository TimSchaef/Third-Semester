using UnityEngine;

public class YSort : MonoBehaviour
{
    public int offset = 0;
    public int multiplier = 100;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        sr.sortingOrder = (int)(-transform.position.y * multiplier) + offset;
    }
}
