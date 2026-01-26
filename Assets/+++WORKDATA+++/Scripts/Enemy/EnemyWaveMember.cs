using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class EnemyWaveMember : MonoBehaviour
{
    private WaveSpawner spawner;
    private HealthComponent hp;
    private bool reportedDeath;

    public void Init(WaveSpawner waveSpawner)
    {
        spawner = waveSpawner;
    }

    private void Awake()
    {
        hp = GetComponent<HealthComponent>();
        if (hp != null)
        {
            hp.OnDeath += HandleDeath;
        }
    }

    private void OnDestroy()
    {
        if (hp != null)
        {
            hp.OnDeath -= HandleDeath;
        }
        
        if (!reportedDeath && spawner != null)
        {
            spawner.OnEnemyKilled(this);
        }
    }

    private void HandleDeath()
    {
        if (reportedDeath) return; 
        reportedDeath = true;

        if (spawner != null)
        {
            spawner.OnEnemyKilled(this);
        }
    }
}

