// TurretSpawner.cs (FINAL: random around player, spread angles + jitter, no overlap)
using System.Collections.Generic;
using UnityEngine;

public class TurretSpawner : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerStatsManager stats;
    [SerializeField] private HealthComponent ownerHealth;

    [Header("Turret Prefab")]
    [SerializeField] private TurretController turretPrefab;

    [Header("Fixed Settings (Inspector)")]
    [SerializeField] private float spawnRadius = 3f;
    [SerializeField] private float turretLifetime = 6f;
    [SerializeField] private float respawnDelay = 4f;
    [SerializeField] private float turretRange = 8f;
    [SerializeField] private float fireRate = 2f; // shots/sec

    [Header("Targeting")]
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private bool ignoreSameRoot = true;

    [Header("Spawn Placement")]
    [SerializeField] private float minDistanceToPlayer = 0.75f;
    [SerializeField] private float minTurretSeparation = 1.25f; // Abstand zwischen Turrets
    [SerializeField] private int maxTriesPerTurret = 30;

    [Tooltip("Wie stark darf der Winkel pro Turret zufällig abweichen (in Grad). 0 = exakt verteilt.")]
    [SerializeField] private float angleJitterDegrees = 25f;

    private readonly List<TurretController> active = new();
    private float respawnTimer;
    private Transform root;

    private void Awake()
    {
        root = transform.root;
        if (!ownerHealth) ownerHealth = GetComponentInParent<HealthComponent>();
    }

    private void Update()
    {
        int count = GetTurretCount();

        if (count <= 0)
        {
            DespawnAll();
            respawnTimer = 0f;
            return;
        }

        CleanupNulls();

        if (active.Count > 0) return;

        if (respawnTimer > 0f)
        {
            respawnTimer -= Time.deltaTime;
            return;
        }

        SpawnSet(count);
    }

    private int GetTurretCount()
    {
        if (!stats) return 0;
        return Mathf.Clamp(Mathf.RoundToInt(stats.GetValue(CoreStatId.TurretCount)), 0, 99);
    }

    private float GetTurretDamage()
    {
        if (!stats) return 0f;
        return Mathf.Max(0f, stats.GetValue(CoreStatId.TurretDamage));
    }

    private void SpawnSet(int count)
    {
        if (!turretPrefab) return;

        float dmg = GetTurretDamage();
        float r = Mathf.Max(0.1f, spawnRadius);

        List<Vector3> positions = BuildSpawnPositions(count, r);

        for (int i = 0; i < positions.Count; i++)
        {
            var t = Instantiate(turretPrefab, positions[i], Quaternion.identity);
            t.Init(new TurretController.Config
            {
                ownerRoot = root,
                ownerHealth = ownerHealth,
                targetLayers = targetLayers,
                ignoreSameRoot = ignoreSameRoot,

                range = turretRange,
                fireRate = fireRate,
                damage = dmg,

                lifetime = turretLifetime,
                onExpired = OnTurretExpired
            });

            active.Add(t);
        }
    }

    private List<Vector3> BuildSpawnPositions(int count, float radius)
    {
        var result = new List<Vector3>(count);
        if (count <= 0) return result;

        float baseStep = 360f / count;
        float startAngle = Random.Range(0f, 360f); // global random rotation each spawn set

        for (int i = 0; i < count; i++)
        {
            // "random but spread": evenly spaced angles + per-turret jitter
            float angle = startAngle + i * baseStep + Random.Range(-angleJitterDegrees, angleJitterDegrees);

            // random distance within ring [minDistanceToPlayer..radius]
            float dist = Random.Range(minDistanceToPlayer, radius);

            // try to keep separation (if fails, we resample dist+angle around this sector)
            if (!TryFindPosNearAngle(angle, dist, radius, result, out Vector3 pos))
            {
                pos = FallbackBestOfSamples(radius, result, samples: 25);
            }

            result.Add(pos);
        }

        return result;
    }

    private bool TryFindPosNearAngle(float baseAngleDeg, float baseDist, float radius, List<Vector3> existing, out Vector3 pos)
    {
        float minSepSqr = minTurretSeparation * minTurretSeparation;

       int tries = Mathf.Max(1, maxTriesPerTurret);
        for (int attempt = 0; attempt < tries; attempt++)
        {
            // resample slightly around the sector to avoid stacking
            float a = baseAngleDeg + Random.Range(-angleJitterDegrees, angleJitterDegrees);
            float d = Mathf.Clamp(baseDist + Random.Range(-0.5f, 0.5f), minDistanceToPlayer, radius);

            Vector3 candidate = transform.position + DirFromAngleDeg(a) * d;

            bool ok = true;
            for (int j = 0; j < existing.Count; j++)
            {
                if ((existing[j] - candidate).sqrMagnitude < minSepSqr)
                {
                    ok = false;
                    break;
                }
            }

            if (ok)
            {
                pos = candidate;
                return true;
            }
        }

        pos = default;
        return false;
    }

    private Vector3 FallbackBestOfSamples(float radius, List<Vector3> existing, int samples)
    {
        Vector3 best = transform.position;
        float bestMinDistSqr = -1f;

        float minSep = Mathf.Max(0.01f, minTurretSeparation);

        for (int i = 0; i < Mathf.Max(1, samples); i++)
        {
            float a = Random.Range(0f, 360f);
            float d = Random.Range(minDistanceToPlayer, radius);
            Vector3 c = transform.position + DirFromAngleDeg(a) * d;

            float minDistSqr = float.PositiveInfinity;
            for (int j = 0; j < existing.Count; j++)
                minDistSqr = Mathf.Min(minDistSqr, (existing[j] - c).sqrMagnitude);

            // even if we can't reach minSep, choose the least-bad one
            if (minDistSqr > bestMinDistSqr && minDistSqr >= (minSep * 0.25f) * (minSep * 0.25f))
            {
                bestMinDistSqr = minDistSqr;
                best = c;
            }
        }

        // If existing is empty, bestMinDistSqr will update; otherwise still okay.
        if (bestMinDistSqr < 0f)
        {
            float a = Random.Range(0f, 360f);
            float d = Random.Range(minDistanceToPlayer, radius);
            best = transform.position + DirFromAngleDeg(a) * d;
        }

        return best;
    }

    private static Vector3 DirFromAngleDeg(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));
    }

    private void OnTurretExpired(TurretController turret)
    {
        active.Remove(turret);

        if (active.Count == 0)
            respawnTimer = Mathf.Max(0f, respawnDelay);
    }

    private void DespawnAll()
    {
        for (int i = active.Count - 1; i >= 0; i--)
            if (active[i]) Destroy(active[i].gameObject);

        active.Clear();
    }

    private void CleanupNulls()
    {
        for (int i = active.Count - 1; i >= 0; i--)
            if (!active[i]) active.RemoveAt(i);
    }
}

