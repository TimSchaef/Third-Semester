using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnEntry
    {
        public GameObject prefab;
        
        public float weight = 1f;
    }

    [Header("Spawn Settings")]
    public EnemySpawnEntry[] enemies;

    [Header("Spawn Area")]
    public Transform areaCenter;
    public Vector3 areaSize = new Vector3(15f, 0, 15f);

    [Header("Player Target")]
    public Transform playerTransform;

    [Header("Spawn Restrictions")]
    public float minDistanceToPlayer = 6f;
    public int maxSpawnTries = 30;

    [Header("Waves")]
    public int startEnemies = 3;
    public int enemiesPerWaveIncrease = 2;

    [Header("Wave Timing")]
    public float baseTimeBetweenWaves = 5f;
    
    public float timeIncreasePerWave = 1.5f;
    
    public float maxTimeBetweenWaves = 0f;

    [Header("Spawn Timing")]
    public float spawnInterval = 0.75f;

    [Header("Enemy Cap")]
    public int maxEnemiesAlive = 0;

    [Header("Difficulty Scaling")]
    public float hpMultiplierPerWave = 0.2f;

    [Header("Spawn Indicator")]
    public GameObject spawnIndicatorPrefab;
    
    public float indicatorDuration = 1.0f;
    
    public float indicatorYOffset = 0.05f;
    
    public Vector3 indicatorRotationEuler = new Vector3(90f, 0f, 0f);

    [Header("Runtime")]
    public int currentWave = 0;
    public int enemiesAlive = 0;

    private void Start()
    {
        StartCoroutine(WaveLoopRoutine());
    }

    private IEnumerator WaveLoopRoutine()
    {
        while (true)
        {
            currentWave++;

            int enemyCount =
                startEnemies + (currentWave - 1) * enemiesPerWaveIncrease;

            for (int i = 0; i < enemyCount; i++)
            {
                if (maxEnemiesAlive > 0)
                {
                    while (enemiesAlive >= maxEnemiesAlive)
                        yield return null;
                }

                SpawnEnemyForCurrentWave();
                yield return new WaitForSeconds(spawnInterval);
            }

            float waitTime = CalculateTimeBetweenWaves();
            yield return new WaitForSeconds(waitTime);
        }
    }

    private float CalculateTimeBetweenWaves()
    {
        float t =
            baseTimeBetweenWaves +
            (currentWave - 1) * timeIncreasePerWave;

        if (maxTimeBetweenWaves > 0f)
            t = Mathf.Min(t, maxTimeBetweenWaves);

        return t;
    }

    private void SpawnEnemyForCurrentWave()
    {
        if (enemies == null || enemies.Length == 0)
            return;

        if (playerTransform == null)
            return;

        if (!TryGetValidSpawnPoint(out Vector3 spawnPos))
            return;

        GameObject prefab = PickWeightedEnemyPrefab();
        if (prefab == null)
            return;

        StartCoroutine(SpawnWithIndicatorRoutine(prefab, spawnPos));
    }

    private IEnumerator SpawnWithIndicatorRoutine(GameObject enemyPrefab, Vector3 spawnPos)
    {
        GameObject indicator = null;

        Vector3 indicatorPos = spawnPos;
        indicatorPos.y += indicatorYOffset;

        if (spawnIndicatorPrefab != null)
        {
            indicator = Instantiate(
                spawnIndicatorPrefab,
                indicatorPos,
                Quaternion.Euler(indicatorRotationEuler)
            );
        }

        if (indicatorDuration > 0f)
            yield return new WaitForSeconds(indicatorDuration);

        if (indicator != null)
            Destroy(indicator);

        GameObject enemyGO =
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        enemiesAlive++;

        
        var dissolve = enemyGO.GetComponent<dissolve2D>();
        if (dissolve != null)
        {
            dissolve.isSpawning = true;
            dissolve.playOnStart = false;
            dissolve.Spawn();
        }
        

        var ai = enemyGO.GetComponent<EnemyMovement>();
        if (ai != null)
            ai.player = playerTransform;

        var member = enemyGO.GetComponent<EnemyWaveMember>();
        if (member != null)
            member.Init(this);

        var hp = enemyGO.GetComponent<HealthComponent>();
        if (hp != null)
        {
            float mult = 1f + hpMultiplierPerWave * (currentWave - 1);
            hp.SetMaxHpMultiplier(mult, true);
        }
    }

    private GameObject PickWeightedEnemyPrefab()
    {
        float total = 0f;

        for (int i = 0; i < enemies.Length; i++)
        {
            var e = enemies[i];
            if (e == null || e.prefab == null) continue;
            if (e.weight <= 0f) continue;
            total += e.weight;
        }

        if (total <= 0f)
            return null;

        float r = Random.Range(0f, total);
        float acc = 0f;

        for (int i = 0; i < enemies.Length; i++)
        {
            var e = enemies[i];
            if (e == null || e.prefab == null) continue;
            if (e.weight <= 0f) continue;

            acc += e.weight;
            if (r <= acc)
                return e.prefab;
        }

        for (int i = enemies.Length - 1; i >= 0; i--)
        {
            var e = enemies[i];
            if (e != null && e.prefab != null && e.weight > 0f)
                return e.prefab;
        }

        return null;
    }

    private bool TryGetValidSpawnPoint(out Vector3 spawnPos)
    {
        Vector3 playerPos = playerTransform.position;

        for (int i = 0; i < maxSpawnTries; i++)
        {
            Vector3 p = GetRandomPointInBox();

            Vector2 p2 = new Vector2(p.x, p.z);
            Vector2 pl2 = new Vector2(playerPos.x, playerPos.z);

            if (Vector2.Distance(p2, pl2) >= minDistanceToPlayer)
            {
                spawnPos = p;
                return true;
            }
        }

        spawnPos = default;
        return false;
    }

    private Vector3 GetRandomPointInBox()
    {
        Vector3 center =
            areaCenter != null ? areaCenter.position : transform.position;

        float halfX = areaSize.x * 0.5f;
        float halfZ = areaSize.z * 0.5f;

        float x = Random.Range(center.x - halfX, center.x + halfX);
        float z = Random.Range(center.z - halfZ, center.z + halfZ);

        return new Vector3(x, center.y, z);
    }

    public void OnEnemyKilled(EnemyWaveMember member)
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center =
            areaCenter != null ? areaCenter.position : transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, new Vector3(areaSize.x, 0.2f, areaSize.z));

        if (playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(
                playerTransform.position,
                minDistanceToPlayer
            );
        }
    }
}












