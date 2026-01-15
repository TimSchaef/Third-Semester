// TurretSpawner.cs
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

    [Header("Spawn Safety")]
    [SerializeField] private float minSpawnDistance = 0.75f;

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

        // Skill nicht freigeschaltet => nichts aktiv
        if (count <= 0)
        {
            DespawnAll();
            respawnTimer = 0f;
            return;
        }

        CleanupNulls();

        // Wenn Turrets aktiv sind: nichts tun (sie laufen aus)
        if (active.Count > 0) return;

        // Keine aktiv: warte auf Respawn Delay
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

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = GetRandomSpawnPos(r);

            var t = Instantiate(turretPrefab, pos, Quaternion.identity);
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

    private Vector3 GetRandomSpawnPos(float radius)
    {
        Vector2 circle = Random.insideUnitCircle;
        if (circle.sqrMagnitude < 0.0001f) circle = Vector2.right;

        Vector3 offset = new Vector3(circle.x, 0f, circle.y).normalized * Random.Range(minSpawnDistance, radius);
        return transform.position + offset;
    }

    private void OnTurretExpired(TurretController turret)
    {
        active.Remove(turret);

        // Wenn letztes Turret weg ist, starte Respawn Delay
        if (active.Count == 0)
            respawnTimer = Mathf.Max(0f, respawnDelay);
    }

    private void DespawnAll()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            if (active[i]) Destroy(active[i].gameObject);
        }
        active.Clear();
    }

    private void CleanupNulls()
    {
        for (int i = active.Count - 1; i >= 0; i--)
            if (!active[i]) active.RemoveAt(i);
    }
}

