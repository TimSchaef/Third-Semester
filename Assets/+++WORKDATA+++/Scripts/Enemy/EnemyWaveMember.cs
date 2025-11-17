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

        // Fallback: falls der Gegner aus irgendeinem Grund zerstört wird,
        // ohne dass OnDeath vorher gefeuert hat, trotzdem eine Meldung schicken.
        if (!reportedDeath && spawner != null)
        {
            spawner.OnEnemyKilled(this);
        }
    }

    private void HandleDeath()
    {
        if (reportedDeath) return; // nur einmal melden
        reportedDeath = true;

        if (spawner != null)
        {
            spawner.OnEnemyKilled(this);
        }
    }
}

