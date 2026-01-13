using UnityEngine;

public class AoeDamageArea : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerStatsManager stats;     // zieht AoeDamage/AoeRadius/AoeTickRate
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private bool ignoreSameRoot = true;

    [Header("Fallback (wenn Stats fehlen)")]
    [SerializeField] private float baseRadius = 2.5f;
    [SerializeField] private float baseDamagePerTick = 5f;
    [SerializeField] private float baseTicksPerSecond = 2f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private Transform root;
    private HealthComponent myHealth;

    private float tickTimer;

    private readonly Collider[] buffer = new Collider[64];

    private void Awake()
    {
        root = transform.root;
        myHealth = GetComponentInParent<HealthComponent>();
    }

    private void Update()
    {
        float ticksPerSec = GetTicksPerSecond();
        float interval = 1f / Mathf.Max(0.1f, ticksPerSec);

        tickTimer -= Time.deltaTime;
        if (tickTimer > 0f) return;

        tickTimer = interval;
        DoTick();
    }

    private void DoTick()
    {
        float radius = GetRadius();
        float dmg = GetDamagePerTick();

        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            radius,
            buffer,
            targetLayers,
            QueryTriggerInteraction.Collide
        );

        if (debugLogs)
            Debug.Log($"[AOE] Tick: radius={radius:0.00}, dmg={dmg:0.0}, hits={count}", this);

        for (int i = 0; i < count; i++)
        {
            var col = buffer[i];
            if (!col) continue;

            if (ignoreSameRoot && col.transform.root == root) continue;

            var hp = col.GetComponent<HealthComponent>() ?? col.GetComponentInParent<HealthComponent>();
            if (!hp || hp.CurrentHP <= 0f) continue;

            hp.ApplyDamage(Mathf.RoundToInt(dmg), myHealth);
        }
    }

    private float GetDamagePerTick()
    {
        float v = baseDamagePerTick;
        if (stats != null) v += stats.GetValue(CoreStatId.AoeDamage);
        return Mathf.Max(0f, v);
    }

    private float GetRadius()
    {
        float v = baseRadius;
        if (stats != null) v += stats.GetValue(CoreStatId.AoeRadius);
        return Mathf.Max(0.1f, v);
    }

    private float GetTicksPerSecond()
    {
        float v = baseTicksPerSecond;
        if (stats != null) v += stats.GetValue(CoreStatId.AoeTickRate);
        return Mathf.Max(0.1f, v);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        float r = baseRadius;
        if (stats != null) r += stats.GetValue(CoreStatId.AoeRadius);
        Gizmos.DrawWireSphere(transform.position, r);
    }
#endif
}
