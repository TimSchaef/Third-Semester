using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public GameObject enemyPrefab;
        public int enemyCount;
        public float spawnInterval;
        public float timeBetweenWaves;
    }

    [Header("Wave Settings")]
    public Wave[] waves;
    public Transform[] spawnPoints;
    
    [Header("Spawn Behavior")]
    public bool randomSpawnPoints = true;
    public float waveStartDelay = 2f;

    private int currentWaveIndex = 0;
    private int enemiesSpawned = 0;
    private int enemiesAlive = 0;
    private bool isSpawning = false;

    void Start()
    {
        StartCoroutine(StartWaveSequence());
    }

    IEnumerator StartWaveSequence()
    {
        yield return new WaitForSeconds(waveStartDelay);
        
        while (currentWaveIndex < waves.Length)
        {
            Wave currentWave = waves[currentWaveIndex];
            
            yield return StartCoroutine(SpawnWave(currentWave));
            
            // Wait for all enemies to be defeated
            while (enemiesAlive > 0)
            {
                yield return new WaitForSeconds(0.5f);
            }
            
            Debug.Log($"Wave {currentWaveIndex + 1} completed!");
            
            currentWaveIndex++;
            
            // Wait before next wave
            if (currentWaveIndex < waves.Length)
            {
                Debug.Log($"Waiting {currentWave.timeBetweenWaves}s before next wave...");
                yield return new WaitForSeconds(currentWave.timeBetweenWaves);
            }
        }
        
        Debug.Log("All waves completed!");
    }

    IEnumerator SpawnWave(Wave wave)
    {
        Debug.Log($"Starting {wave.waveName}");
        isSpawning = true;
        enemiesSpawned = 0;

        for (int i = 0; i < wave.enemyCount; i++)
        {
            SpawnEnemy(wave.enemyPrefab);
            enemiesSpawned++;
            
            if (i < wave.enemyCount - 1)
            {
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }

        isSpawning = false;
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        Transform spawnPoint;
        
        if (randomSpawnPoints)
        {
            spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        }
        else
        {
            spawnPoint = spawnPoints[enemiesSpawned % spawnPoints.Length];
        }

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        
        // Track the enemy GameObject directly
        StartCoroutine(TrackEnemy(enemy));
    }

    IEnumerator TrackEnemy(GameObject enemy)
    {
        enemiesAlive++;
        
        // Wait until the enemy is destroyed
        // Use try-catch to handle the destroyed object gracefully
        while (true)
        {
            if (enemy == null)
            {
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
        
        enemiesAlive--;
    }

    // UI Helper methods
    public int GetCurrentWave()
    {
        return currentWaveIndex + 1;
    }

    public int GetTotalWaves()
    {
        return waves.Length;
    }

    public int GetEnemiesAlive()
    {
        return enemiesAlive;
    }
}