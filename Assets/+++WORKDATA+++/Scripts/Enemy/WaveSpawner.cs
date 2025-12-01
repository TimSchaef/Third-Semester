using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] enemyPrefabs;

    [Header("Spawn Area (Box)")]
    public Transform areaCenter;                
    public Vector3 areaSize = new Vector3(15f, 0, 15f);

    [Header("Player Target")]
    public Transform playerTransform;           

    [Header("Waves")]
    public int startEnemies = 3;
    public int enemiesPerWaveIncrease = 2;

    [Header("Timing")]
    public float timeBetweenWaves = 5f;         // Zeit zwischen Wellen
    public float spawnIntervalInWave = 0.2f;    // Zeit zwischen einzelnen Spawns in einer Welle

    [Header("Difficulty Scaling")]
    public float hpMultiplierPerWave = 0.2f;

    [Header("Runtime")]
    public int currentWave = 0;
    public int enemiesAlive = 0;

    private void Start()
    {
        // Endlos-Wellen-Loop
        StartCoroutine(WaveLoop());
    }

    private IEnumerator WaveLoop()
    {
        while (true)
        {
            // kleine Pause vor der nächsten Welle
            if (currentWave > 0)
                yield return new WaitForSeconds(timeBetweenWaves);

            currentWave++;
            int enemyCount = startEnemies + (currentWave - 1) * enemiesPerWaveIncrease;
            enemiesAlive = 0;

            for (int i = 0; i < enemyCount; i++)
            {
                SpawnEnemyForCurrentWave();
                if (spawnIntervalInWave > 0f)
                    yield return new WaitForSeconds(spawnIntervalInWave);
                else
                    yield return null; // minimaler Delay
            }
        }
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

        var ai = enemyGO.GetComponent<CotLStyleEnemy3D>();
        if (ai != null && playerTransform != null)
        {
            ai.player = playerTransform;
        }

        var member = enemyGO.GetComponent<EnemyWaveMember>();
        if (member != null)
        {
            member.Init(this);
        }

        var hp = enemyGO.GetComponent<HealthComponent>();
        if (hp != null)
        {
            float mult = 1f + hpMultiplierPerWave * (currentWave - 1);
            hp.SetMaxHpMultiplier(mult, true);
        }
    }

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

        // KEIN StartNextWave mehr – Wellen laufen zeitbasiert
        // Hier könntest du nur UI updaten etc.
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = areaCenter != null ? areaCenter.position : transform.position;
        Gizmos.DrawWireCube(center, new Vector3(areaSize.x, 0.2f, areaSize.z));
    }
}






