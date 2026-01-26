using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private HealthComponent health;   
    [SerializeField] private Transform followTarget;   

    [Header("UI")]
    [SerializeField] private Image fillImage;          
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private CanvasGroup visualGroup; 

    [Header("Behavior")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private bool billboardToCamera = true;

    private void Awake()
    {
        if (health == null && followTarget != null)
        {
            health = followTarget.GetComponentInParent<HealthComponent>();
        }

        if (followTarget == null && health != null)
        {
            followTarget = health.transform;
        }

        
        if (health != null)
        {
            health.OnDeath += () => Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        if (!health || !followTarget) return;

        
        UpdateFill();

        
        transform.position = followTarget.position + worldOffset;

       
        if (billboardToCamera && Camera.main)
        {
            Vector3 forward = transform.position - Camera.main.transform.position;
            forward.y = 0f;
            if (forward.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(forward);
        }
    }

    private void UpdateFill()
    {
        if (!fillImage) return;
        if (health.MaxHP <= 0f) return;

        float t = Mathf.Clamp01(health.CurrentHP / health.MaxHP);
        fillImage.fillAmount = t;

        if (hideWhenFull && t >= 0.999f)
        {
            if (visualGroup)
            {
                visualGroup.alpha = 0f;
                visualGroup.interactable = false;
                visualGroup.blocksRaycasts = false;
            }
            else
            {
                fillImage.enabled = false;
            }
        }
        else
        {
            if (visualGroup)
            {
                visualGroup.alpha = 1f;
                visualGroup.interactable = true;
                visualGroup.blocksRaycasts = true;
            }
            else
            {
                fillImage.enabled = true;
            }
        }
    }
}

