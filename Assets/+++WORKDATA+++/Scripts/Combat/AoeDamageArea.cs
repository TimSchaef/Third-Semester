using UnityEngine;

public class AoeDamageArea : MonoBehaviour
{
    [Header("Required Refs")]
    [SerializeField] private PlayerStatsManager stats;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private bool ignoreSameRoot = true;

    [Header("VFX")]
    [SerializeField] private Transform aoeVfx;
    [SerializeField] private float minRadiusToShowVfx = 0f;
    [SerializeField] private float vfxBaseSize = 1f;
    [SerializeField] private bool vfxScaleRepresentsRadius = true;
    [SerializeField] private bool scaleXZOnly = true;

    private Transform root;
    private HealthComponent myHealth;

    private float tickTimer;
    private readonly Collider[] buffer = new Collider[64];

    private float lastVfxScale = -1f;
    private bool vfxVisible;

    private Renderer[] vfxRenderers;
    private ParticleSystem[] vfxParticles;

    private void Awake()
    {
        if (stats == null)
        {
            Debug.LogError("[AoeDamageArea] PlayerStatsManager is REQUIRED.", this);
            enabled = false;
            return;
        }

        root = transform.root;
        myHealth = GetComponentInParent<HealthComponent>();
        CacheVfxComponents();
    }

    private void Update()
    {
        float ticksPerSecond = Mathf.Max(0f, stats.GetValue(CoreStatId.AoeTickRate));
        if (ticksPerSecond <= 0f) return;

        float interval = 1f / ticksPerSecond;
        tickTimer -= Time.deltaTime;

        if (tickTimer <= 0f)
        {
            tickTimer = interval;
            DealDamageTick();
        }
    }

    private void LateUpdate()
    {
        SyncVfx();
    }

    private void DealDamageTick()
    {
        float radius = Mathf.Max(0f, stats.GetValue(CoreStatId.AoeRadius));
        if (radius <= 0f) return;

        float damage = Mathf.Max(0f, stats.GetValue(CoreStatId.AoeDamage));
        if (damage <= 0f) return;

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
            if (hp == null || hp.CurrentHP <= 0f) continue;

            hp.ApplyDamage(Mathf.RoundToInt(damage), myHealth);
        }
    }

    // -------------------------
    // VFX
    // -------------------------

    private void CacheVfxComponents()
    {
        if (aoeVfx == null) return;
        vfxRenderers = aoeVfx.GetComponentsInChildren<Renderer>(true);
        vfxParticles = aoeVfx.GetComponentsInChildren<ParticleSystem>(true);
    }

    private void SetVfxVisible(bool visible)
    {
        if (vfxVisible == visible) return;
        vfxVisible = visible;

        bool wouldDisableSelf = aoeVfx.gameObject == gameObject;

        if (!wouldDisableSelf)
            aoeVfx.gameObject.SetActive(visible);

        if (vfxRenderers != null)
            foreach (var r in vfxRenderers)
                if (r) r.enabled = visible;

        if (vfxParticles != null)
            foreach (var p in vfxParticles)
            {
                if (!p) continue;
                if (visible) p.Play(true);
                else p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
    }

    private void SyncVfx()
    {
        if (aoeVfx == null) return;

        float radius = Mathf.Max(0f, stats.GetValue(CoreStatId.AoeRadius));

        if (radius < minRadiusToShowVfx)
        {
            SetVfxVisible(false);
            return;
        }

        SetVfxVisible(true);

        float desiredWorldSize =
            vfxScaleRepresentsRadius ? radius : radius * 2f;

        float baseSize = Mathf.Max(0.0001f, vfxBaseSize);

        float targetScale = desiredWorldSize / baseSize;

        if (Mathf.Approximately(targetScale, lastVfxScale)) return;
        lastVfxScale = targetScale;

        if (scaleXZOnly)
            aoeVfx.localScale = new Vector3(targetScale, aoeVfx.localScale.y, targetScale);
        else
            aoeVfx.localScale = Vector3.one * targetScale;
    }
}



