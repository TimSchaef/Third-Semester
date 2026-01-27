using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class EnemyAreaBounds : MonoBehaviour
{
    public static EnemyAreaBounds Instance { get; private set; }

    private BoxCollider box;

    private void Awake()
    {
        Instance = this;
        box = GetComponent<BoxCollider>();
    }

    public Vector3 ClampToBounds(Vector3 worldPos)
    {
        Vector3 local = transform.InverseTransformPoint(worldPos);

        local -= box.center;

        Vector3 half = box.size * 0.5f;

        local.x = Mathf.Clamp(local.x, -half.x, half.x);
        local.y = Mathf.Clamp(local.y, -half.y, half.y);
        local.z = Mathf.Clamp(local.z, -half.z, half.z);

        local += box.center;

        return transform.TransformPoint(local);
    }

    public bool Contains(Vector3 worldPos)
    {
        Vector3 local = transform.InverseTransformPoint(worldPos);

        local -= box.center;

        Vector3 half = box.size * 0.5f;

        return
            local.x >= -half.x && local.x <= half.x &&
            local.y >= -half.y && local.y <= half.y &&
            local.z >= -half.z && local.z <= half.z;
    }
}


