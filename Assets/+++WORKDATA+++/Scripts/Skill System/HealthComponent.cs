using UnityEngine;
using System;

public class HealthComponent : MonoBehaviour
{
    public PlayerStatsManager stats;
    public event Action OnDeath;

    public float CurrentHP { get; private set; }
    public float MaxHP => stats.GetValue(CoreStatId.MaxHP);

    void Start() => CurrentHP = MaxHP;

    void Update()
    {
        float regen = Mathf.Max(0f, stats.GetValue(CoreStatId.HPRegen));
        if (regen > 0f && CurrentHP > 0f && CurrentHP < MaxHP)
        {
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + regen * Time.deltaTime);
        }
    }

    public float ApplyDamage(float rawDamage, HealthComponent attacker = null)
    {
        if (CurrentHP <= 0f) return 0f;

        float armor = Mathf.Max(0f, stats.GetValue(CoreStatId.Armor));
        float reduction = armor / (armor + 100f);
        float taken = Mathf.Max(0f, rawDamage * (1f - reduction));

        CurrentHP -= taken;
        if (CurrentHP <= 0f) { CurrentHP = 0f; OnDeath?.Invoke(); }

        // Thorns
        float thornsPct = Mathf.Clamp01(stats.GetValue(CoreStatId.Thorns));
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

