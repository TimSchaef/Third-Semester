using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private Collider attackCollider;     // IsTrigger = true
    [SerializeField] private Transform attackPivot;       // eigener Pivot NUR für AttackBox (optional)
    [SerializeField] private int damage = 10;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float activeTime = 0.25f;
    [SerializeField] private PlayerStatsManager stats;
    [SerializeField] private float critMultiplier = 2f;

    [Header("Cooldown")]
    [SerializeField] private float attackCooldown = 1.0f;

    [Header("Auto Attack")]
    [SerializeField] private bool autoAttack = true;
    [SerializeField] private float autoAttackTick = 0.05f;

    [Header("Targeting")]
    [SerializeField] private float targetRange = 6f;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private bool ignoreSameRoot = true;

    [Header("Facing")]
    [SerializeField] private bool rotateDuringActiveTime = true;
    [SerializeField] private float faceTurnSpeed = 720f;
    [Tooltip("Wenn die Hitbox rückwärts zeigt: 180. Wenn korrekt: 0. Seitlich: 90/-90.")]
    [SerializeField] private float yawOffsetDegrees = 180f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private bool attacking;
    public bool IsAttacking => attacking;

    private Transform root;
    private HealthComponent myHealth;

    private float cooldownTimer = 0f;
    private float autoTimer = 0f;

    private readonly Collider[] overlapBuffer = new Collider[64];

    // =========================
    // === COOLDOWN UI API ===
    // =========================

    /// <summary>0..1: 0 = im Cooldown, 1 = bereit.</summary>
    public float CooldownFill01
    {
        get
        {
            if (attackCooldown <= 0f) return 1f;
            return Mathf.Clamp01(1f - (cooldownTimer / attackCooldown));
        }
    }

    public float RemainingCooldown => Mathf.Max(0f, cooldownTimer);
    public float CooldownDuration => attackCooldown;

    // Pivot-Fallback: wenn attackPivot leer ist, rotiert direkt der Collider-Transform
    private Transform Pivot => attackPivot ? attackPivot : (attackCollider ? attackCollider.transform : null);

    private void Awake()
    {
        root = transform.root;
        myHealth = GetComponentInParent<HealthComponent>();

        if (!attackCollider)
            Debug.LogError("[PlayerAttack] attackCollider ist nicht zugewiesen.", this);

        if (!Pivot)
            Debug.LogError("[PlayerAttack] Weder attackPivot noch attackCollider.transform verfügbar.", this);

        if (attackCollider)
        {
            attackCollider.enabled = false;
            attackCollider.isTrigger = true;
        }
    }

    private void Update()
    {
        // Cooldown runterzählen
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer < 0f) cooldownTimer = 0f;
        }

        // Auto-Attack
        if (autoAttack && !attacking && cooldownTimer <= 0f)
        {
            autoTimer -= Time.deltaTime;
            if (autoTimer <= 0f)
            {
                autoTimer = Mathf.Max(0.01f, autoAttackTick);

                Transform target = FindNearestTarget();
                if (target != null)
                {
                    StartCoroutine(DoAttack(target));
                }
                else if (debugLogs)
                {
                    Debug.Log("[PlayerAttack] Kein Target gefunden (Layer/Range prüfen).", this);
                }
            }
        }
    }

    // Optional: weiterhin per Button möglich
    public void Attack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (attacking) return;
        if (cooldownTimer > 0f) return;

        Transform target = FindNearestTarget();
        if (!target)
        {
            if (debugLogs) Debug.Log("[PlayerAttack] Button: kein Target in Range.", this);
            return;
        }

        StartCoroutine(DoAttack(target));
    }

    private IEnumerator DoAttack(Transform target)
    {
        attacking = true;
        cooldownTimer = attackCooldown;

        FaceTargetInstant(target);

        if (attackCollider) attackCollider.enabled = true;

        float t = 0f;
        while (t < activeTime)
        {
            t += Time.deltaTime;

            if (rotateDuringActiveTime)
                FaceTargetSmooth(target);

            yield return null;
        }

        if (attackCollider) attackCollider.enabled = false;

        attacking = false;
    }

    private Transform FindNearestTarget()
    {
        Vector3 center = transform.position;

        // WICHTIG: Collide, damit Trigger-Collider gefunden werden
        int count = Physics.OverlapSphereNonAlloc(
            center,
            targetRange,
            overlapBuffer,
            targetLayers,
            QueryTriggerInteraction.Collide
        );

        if (debugLogs)
            Debug.Log($"[PlayerAttack] Overlap count = {count} in Range {targetRange}.", this);

        float bestSqr = float.PositiveInfinity;
        Transform best = null;

        for (int i = 0; i < count; i++)
        {
            Collider col = overlapBuffer[i];
            if (!col) continue;

            if (ignoreSameRoot && col.transform.root == root) continue;

            HealthComponent hp =
                col.GetComponent<HealthComponent>() ??
                col.GetComponentInParent<HealthComponent>();

            if (!hp || hp.CurrentHP <= 0f) continue;

            float sqr = (hp.transform.position - center).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = hp.transform;
            }
        }

        return best;
    }

    private void FaceTargetInstant(Transform target)
    {
        Transform p = Pivot;
        if (!p || !target) return;

        Vector3 dir = target.position - p.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion desired =
            Quaternion.LookRotation(dir.normalized, Vector3.up) *
            Quaternion.Euler(0f, yawOffsetDegrees, 0f);

        p.rotation = desired;

        if (debugLogs)
            Debug.Log($"[PlayerAttack] FaceTargetInstant -> {target.name}", this);
    }

    private void FaceTargetSmooth(Transform target)
    {
        Transform p = Pivot;
        if (!p || !target) return;

        Vector3 dir = target.position - p.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion desired =
            Quaternion.LookRotation(dir.normalized, Vector3.up) *
            Quaternion.Euler(0f, yawOffsetDegrees, 0f);

        p.rotation = Quaternion.RotateTowards(p.rotation, desired, faceTurnSpeed * Time.deltaTime);
    }
    
    private int CalculateDamage()
    {
        float baseDamage = damage;

        // Damage aus Stats (Add)
        if (stats != null)
            baseDamage += stats.GetValue(CoreStatId.Damage);

        // CritChance aus Stats (0..1)
        float critChance = 0f;
        if (stats != null)
            critChance = Mathf.Clamp01(stats.GetValue(CoreStatId.CritChance));

        bool isCrit = Random.value < critChance;
        if (isCrit)
            baseDamage *= critMultiplier;

        return Mathf.RoundToInt(baseDamage);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!attacking) return;
        if (!IsInMask(other.gameObject.layer, targetLayers)) return;
        if (ignoreSameRoot && other.transform.root == root) return;

        var hp = other.GetComponent<HealthComponent>() ?? other.GetComponentInParent<HealthComponent>();
        if (!hp) return;

        int finalDamage = CalculateDamage();
        hp.ApplyDamage(finalDamage, myHealth);

        var rb = other.attachedRigidbody;
        if (rb)
        {
            Vector3 dir = (other.transform.position - transform.position).normalized;
            dir.y = 0f;
            rb.AddForce(dir * knockbackForce, ForceMode.Impulse);
        }
    }

    private static bool IsInMask(int layer, LayerMask mask)
        => (mask.value & (1 << layer)) != 0;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, targetRange);
    }
#endif
}










