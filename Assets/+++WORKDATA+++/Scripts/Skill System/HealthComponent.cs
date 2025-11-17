using UnityEngine;
using System;

public class HealthComponent : MonoBehaviour
{
    public PlayerStatsManager stats;
    public event Action OnDeath;

    [Header("Scaling")]
    [Tooltip("Multiplikator für MaxHP (z.B. für Wave-Skalierung). 1 = normal.")]
    [SerializeField] private float maxHpMultiplier = 1f;
    public float MaxHpMultiplier => maxHpMultiplier;

    public float CurrentHP { get; private set; }

    // Basis-MaxHP aus Stats:
    public float BaseMaxHP => stats != null ? stats.GetValue(CoreStatId.MaxHP) : 0f;

    // Effektives MaxHP (inkl. Multiplikator)
    public float MaxHP => BaseMaxHP * Mathf.Max(0.1f, maxHpMultiplier);

    void Start()
    {
        // Beim Start vollheilen
        CurrentHP = MaxHP;
    }

    void Update()
    {
        float regen = Mathf.Max(0f, stats != null ? stats.GetValue(CoreStatId.HPRegen) : 0f);
        if (regen > 0f && CurrentHP > 0f && CurrentHP < MaxHP)
        {
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + regen * Time.deltaTime);
        }
    }

    /// <summary>
    /// Wird z.B. vom WaveSpawner aufgerufen, um Gegner-HP für höhere Wellen zu skalieren.
    /// </summary>
    public void SetMaxHpMultiplier(float mult, bool healToFull = true)
    {
        maxHpMultiplier = Mathf.Max(0.1f, mult);
        if (healToFull)
        {
            CurrentHP = MaxHP;
        }
    }

    public float ApplyDamage(float rawDamage, HealthComponent attacker = null)
    {
        if (CurrentHP <= 0f) return 0f;

        float armor = Mathf.Max(0f, stats != null ? stats.GetValue(CoreStatId.Armor) : 0f);
        float reduction = armor / (armor + 100f);
        float taken = Mathf.Max(0f, rawDamage * (1f - reduction));

        CurrentHP -= taken;
        if (CurrentHP <= 0f) { CurrentHP = 0f; OnDeath?.Invoke(); }

        // Thorns
        float thornsPct = stats != null ? Mathf.Clamp01(stats.GetValue(CoreStatId.Thorns)) : 0f;
        if (thornsPct > 0f && attacker != null && attacker.CurrentHP > 0f)
        {
            float reflect = taken * thornsPct;
            attacker.ApplyPureDamage(reflect); // Thorns ignoriert Armor hier (kannst du anpassen)
        }

        return taken;
    }

    public void ApplyPureDamage(float amount)
    {
        if (CurrentHP <= 0f) return;
        CurrentHP -= amount;
        if (CurrentHP <= 0f) { CurrentHP = 0f; OnDeath?.Invoke(); }
    }

    public void Heal(float amount)
    {
        if (CurrentHP <= 0f) return;
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
    }
}
