using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStatsManager stats;     // Player Stats Manager
    [SerializeField] private Hitpoints selfHealth;         // eigene HP-Komponente (für LifeSteal-Heal)

    [Header("Attack Hitbox")]
    [SerializeField] private Collider attackCollider;      // IsTrigger = true
    [SerializeField] private LayerMask targetLayers;       // Ziel-Layer ("Enemy")
    [SerializeField] private bool ignoreSameRoot = true;

    [Header("Base Settings")]
    [SerializeField] private int baseDamage = 10;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float activeTime = 0.25f;
    [SerializeField] private float baseCooldown = 0.6f;    // wird durch AttackSpeed geteilt

    private bool attacking;
    private float cooldownTimer;
    private float lastCooldownDuration;
    private Transform _root;

    // --- UI Support (Cooldown Indicator) ---
    public float CooldownRemaining => Mathf.Max(0f, cooldownTimer);
    public float LastCooldownDuration => lastCooldownDuration;
    public float CooldownFill01
    {
        get
        {
            if (lastCooldownDuration <= 0f) return 1f;
            float remaining = Mathf.Max(0f, cooldownTimer);
            return 1f - Mathf.Clamp01(remaining / lastCooldownDuration);
        }
    }

    private void Awake()
    {
        _root = transform.root;
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
            cooldownTimer -= Time.deltaTime;
    }

    public void Attack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || attacking) return;
        if (cooldownTimer > 0f) return;

        float atkSpeed = stats ? Mathf.Max(0.01f, stats.GetValue(CoreStatId.AttackSpeed)) : 1f;
        lastCooldownDuration = baseCooldown / atkSpeed;
        cooldownTimer = lastCooldownDuration;

        StartCoroutine(DoAttack());
    }

    private IEnumerator DoAttack()
    {
        attacking = true;
        if (attackCollider) attackCollider.enabled = true;

        yield return new WaitForSeconds(activeTime);

        if (attackCollider) attackCollider.enabled = false;
        attacking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!attacking) return;
        if (!IsInMask(other.gameObject.layer, targetLayers)) return;
        if (ignoreSameRoot && other.transform.root == _root) return;

        var hp = other.GetComponent<Hitpoints>() ?? other.GetComponentInParent<Hitpoints>();
        if (!hp) return;

        Vector3 dir = (other.transform.position - transform.position).normalized;

        int dmg = baseDamage;

        hp.TakeDamage(
            dmg,
            dir,
            knockbackForce,
            attackerStats: stats,
            attackerHealth: selfHealth
        );
    }

    private static bool IsInMask(int layer, LayerMask mask)
        => (mask.value & (1 << layer)) != 0;
}


