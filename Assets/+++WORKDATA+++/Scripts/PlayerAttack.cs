using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private Collider attackCollider; // IsTrigger = true
    [SerializeField] private int damage = 10;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float activeTime = 0.25f;

    [Header("Cooldown")]
    [SerializeField] private float attackCooldown = 1.0f; // Sekunden zwischen Angriffen

    [Header("Filter")]
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private bool ignoreSameRoot = true;

    private bool attacking;
    private Transform _root;
    private HealthComponent _myHealth;
    private float cooldownTimer = 0f;

    /// <summary>
    /// 0..1: 0 = im Cooldown, 1 = bereit.
    /// </summary>
    public float CooldownFill01
    {
        get
        {
            if (attackCooldown <= 0f) return 1f;
            float t = Mathf.Clamp01(1f - (cooldownTimer / attackCooldown));
            return t;
        }
    }

    public float RemainingCooldown => Mathf.Max(0f, cooldownTimer);
    public float CooldownDuration => attackCooldown;

    private void Awake()
    {
        _root = transform.root;
        _myHealth = GetComponentInParent<HealthComponent>();

        if (attackCollider)
        {
            attackCollider.enabled = false;
            attackCollider.isTrigger = true;
            attackCollider.gameObject.layer = LayerMask.NameToLayer("PlayerAttack");
        }
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer < 0f) cooldownTimer = 0f;
        }
    }

    public void Attack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (attacking) return;
        if (cooldownTimer > 0f) return;

        StartCoroutine(DoAttack());
    }

    private System.Collections.IEnumerator DoAttack()
    {
        attacking = true;
        cooldownTimer = attackCooldown;

        if (attackCollider)
            attackCollider.enabled = true;

        yield return new WaitForSeconds(activeTime);

        if (attackCollider)
            attackCollider.enabled = false;

        attacking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!attacking) return;
        if (!IsInMask(other.gameObject.layer, targetLayers)) return;
        if (ignoreSameRoot && other.transform.root == _root) return;

        var hp = other.GetComponent<HealthComponent>() ?? other.GetComponentInParent<HealthComponent>();
        if (!hp) return;

        Vector3 dir = (other.transform.position - transform.position).normalized;

        // Schaden + Armor + Thorns + LifeSteal-on-kill
        hp.ApplyDamage(damage, _myHealth);

        // Physischer Knockback
        var rb = other.attachedRigidbody;
        if (rb != null)
        {
            dir.y = 0f;
            rb.AddForce(dir.normalized * knockbackForce, ForceMode.Impulse);
        }
    }

    private static bool IsInMask(int layer, LayerMask mask) => (mask.value & (1 << layer)) != 0;
}





