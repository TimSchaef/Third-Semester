using UnityEngine;

public class AoeDamageArea : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerStatsManager stats;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private bool ignoreSameRoot = true;

    [Header("Fallback (wenn Stats fehlen)")]
    [SerializeField] private float fallbackRadius = 2.5f;
    [SerializeField] private float fallbackDamagePerTick = 5f;   // wird ignoriert, wenn Stats gesetzt sind
    [SerializeField] private float fallbackTicksPerSecond = 2f;

    private Transform root;
    private HealthComponent myHealth;

    private float timer;
    private readonly Collider[] buffer = new Collider[64];

    private void Awake()
    {
        root = transform.root;
        myHealth = GetComponentInParent<HealthComponent>();
    }

    private void Update()
    {
        float ticksPerSecond = GetTicksPerSecond();
        float interval = 1f / Mathf.Max(0.1f, ticksPerSecond);

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        timer = interval;
        Tick();
    }

    private void Tick()
    {
        float radius = GetRadius();
        float dmg = GetAoeDamageOnly(); // NUR AoeDamage!

        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            radius,
            buffer,
            targetLayers,
            QueryTriggerInteraction.Collide
        );

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

    // -------------------------
    // AOE Werte (separat)
    // -------------------------

    public float GetAoeDamageOnly()
    {
        // Wenn du es 100% stat-only willst:
        // return stats != null ? Mathf.Max(0f, stats.GetValue(CoreStatId.AoeDamage)) : 0f;

        // Falls du lieber Fallback willst, wenn stats null ist:
        if (stats == null) return Mathf.Max(0f, fallbackDamagePerTick);
        return Mathf.Max(0f, stats.GetValue(CoreStatId.AoeDamage));
    }

    public float GetRadius()
    {
        if (stats == null) return Mathf.Max(0.1f, fallbackRadius);
        return Mathf.Max(0.1f, stats.GetValue(CoreStatId.AoeRadius));
    }

    private float GetTicksPerSecond()
    {
        if (stats == null) return Mathf.Max(0.1f, fallbackTicksPerSecond);
        return Mathf.Max(0.1f, stats.GetValue(CoreStatId.AoeTickRate));
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        float radius = Application.isPlaying ? GetRadius() : fallbackRadius;
        Gizmos.color = new Color(1f, 0f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}

