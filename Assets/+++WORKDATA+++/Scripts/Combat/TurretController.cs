// TurretController.cs
using System;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    [Serializable]
    public struct Config
    {
        public Transform ownerRoot;
        public HealthComponent ownerHealth;
        public LayerMask targetLayers;
        public bool ignoreSameRoot;

        public float range;
        public float fireRate; // shots/sec
        public float damage;

        public float lifetime;
        public Action<TurretController> onExpired;
    }

    private Config cfg;
    private float lifeTimer;
    private float shotTimer;

    private Camera cam;

    private readonly Collider[] buffer = new Collider[64];

    public void Init(Config config)
    {
        cfg = config;
        lifeTimer = Mathf.Max(0.1f, cfg.lifetime);
        shotTimer = 0f;
        cam = Camera.main;
    }

    private void Update()
    {
        FaceCamera(); // <<< immer zur Kamera, NICHT zum Target

        // lifetime
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            cfg.onExpired?.Invoke(this);
            Destroy(gameObject);
            return;
        }

        // shooting cadence
        float interval = 1f / Mathf.Max(0.1f, cfg.fireRate);
        shotTimer -= Time.deltaTime;
        if (shotTimer > 0f) return;
        shotTimer = interval;

        var target = FindNearestTarget();
        if (!target) return;

        // Schaden (kein Drehen zum Target)
        target.ApplyDamage(cfg.damage, cfg.ownerHealth);
    }

    private void FaceCamera()
    {
        if (!cam) cam = Camera.main;
        if (!cam) return;

        // Yaw-only billboard: steht stabil auf dem Boden
        Vector3 dir = cam.transform.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
    }

    private HealthComponent FindNearestTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            Mathf.Max(0.1f, cfg.range),
            buffer,
            cfg.targetLayers,
            QueryTriggerInteraction.Collide
        );

        float bestSqr = float.PositiveInfinity;
        HealthComponent best = null;

        for (int i = 0; i < count; i++)
        {
            var col = buffer[i];
            if (!col) continue;

            if (cfg.ignoreSameRoot && cfg.ownerRoot && col.transform.root == cfg.ownerRoot)
                continue;

            var hp = col.GetComponent<HealthComponent>() ?? col.GetComponentInParent<HealthComponent>();
            if (!hp || hp.CurrentHP <= 0f) continue;

            float sqr = (hp.transform.position - transform.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = hp;
            }
        }

        return best;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, cfg.range);
    }
#endif
}


