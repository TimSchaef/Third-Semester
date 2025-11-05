using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;          // Zuweisen: Filled Image
    [SerializeField] private bool hideWhenFull = true; // bei 100% ausblenden
    [SerializeField] private CanvasGroup visualGroup;  // auf denselben UI-Container legen (nicht auf das Script-Objekt)

    [Header("Behavior")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private bool billboardToCamera = true;

    private int maxHP = 1;
    private Transform target;

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
        {
            if (visualGroup) { visualGroup.alpha = 0f; visualGroup.interactable = false; visualGroup.blocksRaycasts = false; }
            else             { fillImage.enabled = false; }
        }
        else
        {
            if (visualGroup) { visualGroup.alpha = 1f; visualGroup.interactable = true; visualGroup.blocksRaycasts = true; }
            else             { fillImage.enabled = true; }
        }
    }

    private void LateUpdate()
    {
        if (!target) return;

        // folgen
        transform.position = target.position + worldOffset;

        // zur Kamera drehen
        if (billboardToCamera && Camera.main)
        {
            Vector3 forward = transform.position - Camera.main.transform.position;
            forward.y = 0f;
            if (forward.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(forward);
        }
    }
}
