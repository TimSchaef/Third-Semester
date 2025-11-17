using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] enemyPrefabs;   // Gegner-Prefabs (mit HealthComponent + EnemyWaveMember)
    [SerializeField] private Transform[] spawnPoints;     // mögliche Spawnpunkte

    [Header("Waves")]
    [SerializeField] private int startEnemies = 3;        // Anzahl Gegner in Welle 1
    [SerializeField] private int enemiesPerWaveIncrease = 2; // pro Welle kommen so viele dazu
    [SerializeField] private float timeBetweenWaves = 5f;     // Pause zwischen Wellen

    [Header("Difficulty")]
    [Tooltip("Zusätzlicher HP-Multiplikator pro Welle: 0.2 = +20% HP pro Welle")]
    [SerializeField] private float hpMultiplierPerWave = 0.2f;

    [Header("State (read-only)")]
    [SerializeField] private int currentWave = 0;
    [SerializeField] private int enemiesAlive = 0;
    [SerializeField] private bool autoStart = true;

    private bool isSpawning = false;

    private void Start()
    {
        if (autoStart)
        {
            StartNextWave();
        }
    }
    

    public void StartNextWave()
    {
        if (isSpawning) return;
        StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        isSpawning = true;

        // Ab Welle 2 Pause einlegen
        if (currentWave > 0)
        {
            Debug.Log($"Next wave in {timeBetweenWaves} seconds...");
            yield return new WaitForSeconds(timeBetweenWaves);
        }

        currentWave++;

        int enemyCount = startEnemies + (currentWave - 1) * enemiesPerWaveIncrease;
        Debug.Log($"Spawning Wave {currentWave} with {enemyCount} enemies.");

        enemiesAlive = 0;

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemyForCurrentWave();
            yield return new WaitForSeconds(0.1f); // minimaler Delay, optional
        }

        isSpawning = false;
    }

    private void SpawnEnemyForCurrentWave()
    {
        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject enemyGO = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        enemiesAlive++;

        var member = enemyGO.GetComponent<EnemyWaveMember>();
        if (member != null) member.Init(this);

        // 🔥 FIX: Gegner bekommt nach dem Spawn sofort den Player als Target
        var ai = enemyGO.GetComponent<CotLStyleEnemy3D>();
        if (ai != null)
            ai.player = GameObject.FindWithTag("Player").transform;

        // HP Scaling usw…
        var hp = enemyGO.GetComponent<HealthComponent>();
        if (hp != null)
        {
            float mult = 1f + hpMultiplierPerWave * (currentWave - 1);
            hp.SetMaxHpMultiplier(mult, healToFull: true);
        }
    }

    // Wird vom EnemyWaveMember bei OnDeath aufgerufen
    public void OnEnemyKilled(EnemyWaveMember member)
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        Debug.Log($"Enemy killed. Remaining: {enemiesAlive}");

        if (enemiesAlive == 0)
        {
            Debug.Log($"Wave {currentWave} cleared!");
            StartNextWave();  // nächste Welle nach Delay-Coroutine
        }
    }
}
