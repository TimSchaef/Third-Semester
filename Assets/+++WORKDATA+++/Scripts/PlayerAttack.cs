using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

[RequireComponent(typeof(Collider))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private Collider attackCollider;     // IsTrigger = true
    [SerializeField] private Transform attackPivot;       // eigener Pivot NUR für AttackBox (optional)
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float activeTime = 0.25f;

    [Header("Cooldown (Base)")]
    [Tooltip("Basis-Cooldown bei AttackSpeed = 1.0")]
    [SerializeField] private float baseAttackCooldown = 1.0f;

    [Header("Stats (required for Damage)")]
    [Tooltip("MUSS gesetzt sein, weil Damage vollständig aus CoreStatId.Damage kommt.")]
    [SerializeField] private PlayerStatsManager stats;

    [Header("Auto Attack")]
    [SerializeField] private bool autoAttack = true;
    [SerializeField] private float autoAttackTick = 0.05f;

    [Header("Targeting")]
    [SerializeField] private float targetRange = 6f;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private bool ignoreSameRoot = true;

    [Header("Facing (Hitbox)")]
    [SerializeField] private bool rotateDuringActiveTime = true;
    [SerializeField] private float faceTurnSpeed = 720f;
    [Tooltip("Wenn die Hitbox rückwärts zeigt: 180. Wenn korrekt: 0. Seitlich: 90/-90.")]
    [SerializeField] private float yawOffsetDegrees = 180f;

    [Header("VFX Graph (Existing VisualEffect)")]
    [Tooltip("Ziehe hier das VisualEffect-Objekt aus der Szene rein (VFX Graph).")]
    [SerializeField] private VisualEffect attackVfx;

    [Tooltip("Optional: Wo die VFX sitzen soll. Wenn leer -> Pivot (attackPivot oder attackCollider.transform).")]
    [SerializeField] private Transform vfxPivot;

    [Tooltip("Wenn true, wird die VFX beim Attack auf das Target ausgerichtet.")]
    [SerializeField] private bool vfxAlignToTarget = true;

    [Tooltip("Eigener Yaw-Offset nur fuer VFX. Nutze 180, wenn die VFX entgegengesetzt zeigt.")]
    [SerializeField] private float vfxYawOffsetDegrees = 0f;

    [Tooltip("Wenn true, wird beim Triggern ein Stop() gemacht (Reset), dann Play().")]
    [SerializeField] private bool vfxResetBeforePlay = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private bool attacking;
    public bool IsAttacking => attacking;

    private Transform root;
    private HealthComponent myHealth;

    private float cooldownTimer = 0f;
    private float autoTimer = 0f;

    private readonly Collider[] overlapBuffer = new Collider[64];

    // Pivot-Fallback: wenn attackPivot leer ist, rotiert direkt der Collider-Transform
    private Transform Pivot => attackPivot ? attackPivot : (attackCollider ? attackCollider.transform : null);
    private Transform VfxPivot => vfxPivot ? vfxPivot : Pivot;

    private void Awake()
    {
        root = transform.root;
        myHealth = GetComponentInParent<HealthComponent>();

        if (!attackCollider)
            Debug.LogError("[PlayerAttack] attackCollider ist nicht zugewiesen.", this);

        if (!Pivot)
            Debug.LogError("[PlayerAttack] Weder attackPivot noch attackCollider.transform verfuegbar.", this);

        if (!stats)
            Debug.LogError("[PlayerAttack] stats ist NICHT gesetzt, aber Damage kommt aus Stats.", this);

        if (attackCollider)
        {
            attackCollider.enabled = false;
            attackCollider.isTrigger = true;
        }

        if (attackVfx)
            attackVfx.Stop();
    }

    private void Update()
    {
        // Cooldown runterzaehlen
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
                    Debug.Log("[PlayerAttack] Kein Target gefunden (Layer/Range pruefen).", this);
                }
            }
        }
    }

    // Optional: weiterhin per Button moeglich
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

        // Cooldown uses AttackSpeed
        cooldownTimer = GetEffectiveCooldown();

        // Hitbox ausrichten
        FaceTargetInstant(target);

        // VFX triggern
        PlayAttackVfx(target);

        if (attackCollider) attackCollider.enabled = true;

        float t = 0f;
        while (t < activeTime)
        {
            t += Time.deltaTime;

            if (rotateDuringActiveTime)
            {
                FaceTargetSmooth(target);

                if (vfxAlignToTarget)
                    AlignVfxToTarget(target);
            }

            yield return null;
        }

        if (attackCollider) attackCollider.enabled = false;

        attacking = false;
    }

    // ---------------------------
    // Damage kommt ausschließlich aus CoreStatId.Damage
    // ---------------------------
    private float GetFinalDamageFromStats()
    {
        if (!stats) return 0f;
        // Dein System: (baseVal + add) * (1 + mult)
        // => baseVal muss bei Damage sinnvoll gesetzt sein
        return Mathf.Max(0f, stats.GetValue(CoreStatId.Damage));
    }

    // ---------------------------
    // VFX Graph Helpers
    // ---------------------------
    private void PlayAttackVfx(Transform target)
    {
        if (!attackVfx) return;

        Transform p = VfxPivot;
        if (p)
        {
            attackVfx.transform.position = p.position;
            attackVfx.transform.rotation = p.rotation;
        }

        if (vfxAlignToTarget)
            AlignVfxToTarget(target);

        if (vfxResetBeforePlay)
            attackVfx.Stop();

        attackVfx.Play();

        if (debugLogs)
            Debug.Log("[PlayerAttack] VFX Play()", this);
    }

    private void AlignVfxToTarget(Transform target)
    {
        if (!attackVfx || !target) return;

        Transform p = VfxPivot ? VfxPivot : attackVfx.transform;

        Vector3 dir = target.position - p.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion desired =
            Quaternion.LookRotation(dir.normalized, Vector3.up) *
            Quaternion.Euler(0f, vfxYawOffsetDegrees, 0f);

        attackVfx.transform.rotation = desired;
    }

    // ---------------------------
    // AttackSpeed -> Cooldown
    // ---------------------------
    private float GetEffectiveCooldown()
    {
        float atkSpeed = 1f;

        if (stats != null)
            atkSpeed = stats.GetValue(CoreStatId.AttackSpeed);

        atkSpeed = Mathf.Max(0.01f, atkSpeed); // safety
        return Mathf.Max(0.01f, baseAttackCooldown) / atkSpeed;
    }

    private Transform FindNearestTarget()
    {
        Vector3 center = transform.position;

        int count = Physics.OverlapSphereNonAlloc(
            center,
            targetRange,
            overlapBuffer,
            targetLayers,
            QueryTriggerInteraction.Collide
        );

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

    private void OnTriggerEnter(Collider other)
    {
        if (!attacking) return;
        if (!IsInMask(other.gameObject.layer, targetLayers)) return;
        if (ignoreSameRoot && other.transform.root == root) return;

        var hp = other.GetComponent<HealthComponent>() ?? other.GetComponentInParent<HealthComponent>();
        if (!hp) return;

        float finalDamage = GetFinalDamageFromStats();
        if (finalDamage > 0f)
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













