using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;          // assign a filled Image
    [SerializeField] private bool hideWhenFull = true; // auto-hide at 100%

    [Header("Behavior")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private bool billboardToCamera = true;

    private int maxHP = 1;
    private Transform target; // usually the enemy root

    public void Init(Transform followTarget, int maxHitPoints, int currentHitPoints)
    {
        target = followTarget;
        maxHP = Mathf.Max(1, maxHitPoints);
        Set(currentHitPoints);
    }

    public void Set(int currentHitPoints)
    {
        if (!fillImage) return;

        float t = Mathf.Clamp01((float)currentHitPoints / Mathf.Max(1, maxHP));
        fillImage.fillAmount = t;

        if (hideWhenFull && t >= 0.999f)
            fillImage.transform.parent.gameObject.SetActive(false);
        else
            fillImage.transform.parent.gameObject.SetActive(true);
    }

    private void LateUpdate()
    {
        if (!target) return;

        // follow enemy
        transform.position = target.position + worldOffset;

        // face camera
        if (billboardToCamera && Camera.main)
        {
            Vector3 forward = transform.position - Camera.main.transform.position;
            forward.y = 0f;
            if (forward.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(forward);
        }
    }
}
