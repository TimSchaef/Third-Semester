using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] enemyPrefabs;

    [Header("Spawn Area (Box)")]
    public Transform areaCenter;                // Mittelpunkt der Box
    public Vector3 areaSize = new Vector3(15f, 0, 15f);

    [Header("Player Target")]
    public Transform playerTransform;           // ← HIER dein echter Player rein!

    [Header("Waves")]
    public int startEnemies = 3;
    public int enemiesPerWaveIncrease = 2;
    public float timeBetweenWaves = 5f;

    [Header("Difficulty Scaling")]
    public float hpMultiplierPerWave = 0.2f;

    [Header("Runtime")]
    public int currentWave = 0;
    public int enemiesAlive = 0;
    private bool isSpawning = false;

    private void Start()
    {
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (!isSpawning)
            StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        isSpawning = true;

        if (currentWave > 0)
            yield return new WaitForSeconds(timeBetweenWaves);

        currentWave++;
        int enemyCount = startEnemies + (currentWave - 1) * enemiesPerWaveIncrease;

        enemiesAlive = 0;

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemyForCurrentWave();
            yield return null;  // Mini Delay, verteilt die Spawns
        }

        isSpawning = false;
    }

    private void SpawnEnemyForCurrentWave()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("WaveSpawner: No enemyPrefabs assigned!");
            return;
        }

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        Vector3 spawnPos = GetRandomPointInBox();
        GameObject enemyGO = Instantiate(prefab, spawnPos, Quaternion.identity);

        enemiesAlive++;

        // 🟢 Gegner bekommt IMMER das richtige Player-Transform
        var ai = enemyGO.GetComponent<CotLStyleEnemy3D>();
        if (ai != null && playerTransform != null)
        {
            ai.player = playerTransform;   // ← FIX: Gegner tracken immer den echten Player
        }

        // Optional: WaveMember für OnEnemyKilled
        var member = enemyGO.GetComponent<EnemyWaveMember>();
        if (member != null)
        {
            member.Init(this);
        }

        // optional HP-Skalierung pro Welle
        var hp = enemyGO.GetComponent<HealthComponent>();
        if (hp != null)
        {
            float mult = 1f + hpMultiplierPerWave * (currentWave - 1);
            hp.SetMaxHpMultiplier(mult, true);
        }
    }

    // Zufallsposition in der Box-Area (rechteckig)
    private Vector3 GetRandomPointInBox()
    {
        Vector3 center = areaCenter != null ? areaCenter.position : transform.position;

        float halfX = areaSize.x * 0.5f;
        float halfZ = areaSize.z * 0.5f;

        float x = Random.Range(center.x - halfX, center.x + halfX);
        float z = Random.Range(center.z - halfZ, center.z + halfZ);

        return new Vector3(x, center.y, z);
    }

    public void OnEnemyKilled(EnemyWaveMember member)
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);

        if (enemiesAlive == 0)
        {
            StartNextWave();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = areaCenter != null ? areaCenter.position : transform.position;
        Gizmos.DrawWireCube(center, new Vector3(areaSize.x, 0.2f, areaSize.z));
    }
}





