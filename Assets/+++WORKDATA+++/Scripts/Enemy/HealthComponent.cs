using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthComponent : MonoBehaviour
{
    public PlayerStatsManager stats;
    public event Action OnDeath;

    [Header("Fallback HP")]
    [Tooltip("Wird benutzt, wenn kein Stats-Objekt vorhanden ist oder MaxHP aus Stats <= 0 ist.")]
    [SerializeField] private float fallbackMaxHP = 100f;

    [Header("Scaling")]
    [Tooltip("Multiplikator für MaxHP (z.B. für Wellen-Skalierung). 1 = normal.")]
    [SerializeField] private float maxHpMultiplier = 1f;
    public float MaxHpMultiplier => maxHpMultiplier;

    public float CurrentHP { get; private set; }

  
    public float BaseMaxHP
    {
        get
        {
            float baseFromStats = 0f;
            if (stats != null)
                baseFromStats = stats.GetValue(CoreStatId.MaxHP);
            
            if (baseFromStats <= 0f)
                baseFromStats = fallbackMaxHP;

            return baseFromStats;
        }
    }
    
    public float MaxHP => BaseMaxHP * Mathf.Max(0.1f, maxHpMultiplier);

    private bool isDead = false;

    void Start()
    {
        float max = MaxHP;
        if (max <= 0f)
        {
            maxHpMultiplier = 1f;
            max = fallbackMaxHP;
        }

        CurrentHP = max;
    }

    void Update()
    {
        if (isDead) return;

        float regen = 0f;
        if (stats != null)
            regen = Mathf.Max(0f, stats.GetValue(CoreStatId.HPRegen));

        if (regen > 0f && CurrentHP > 0f && CurrentHP < MaxHP)
        {
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + regen * Time.deltaTime);
        }
    }
    
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
        if (isDead || CurrentHP <= 0f) return 0f;

       
        float armor = 0f;
        if (stats != null)
            armor = Mathf.Max(0f, stats.GetValue(CoreStatId.Armor));

        float reduction = armor / (armor + 100f);
        float taken = Mathf.Max(0f, rawDamage * (1f - reduction));

        if (taken <= 0f) return 0f;

        float previousHP = CurrentHP;
        CurrentHP -= taken;

        bool diedThisHit = previousHP > 0f && CurrentHP <= 0f;

        
        float thornsPct = 0f;
        if (stats != null)
            thornsPct = Mathf.Clamp01(stats.GetValue(CoreStatId.Thorns));

        if (thornsPct > 0f && attacker != null && !attacker.isDead && attacker.CurrentHP > 0f)
        {
            float reflect = taken * thornsPct;
            attacker.ApplyPureDamage(reflect);
        }

        
        if (diedThisHit && attacker != null && attacker.stats != null)
        {
            float lsPct = Mathf.Clamp01(attacker.stats.GetValue(CoreStatId.LifeSteal));
            if (lsPct > 0f)
            {
                float healAmount = taken * lsPct;
                attacker.Heal(healAmount);
            }
        }

        
        if (CompareTag("Player"))
        {
            if (Camera.main != null)
            {
                Camera.main.DOShakePosition(0.5f, new Vector3(0.2f, 0.2f, 0f));
            }
        }

        if (diedThisHit)
            Die();

        return taken;
    }

    public void ApplyPureDamage(float amount)
    {
        if (isDead || CurrentHP <= 0f) return;

        CurrentHP -= amount;
        if (CurrentHP <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead || CurrentHP <= 0f) return;
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        CurrentHP = 0f;

        OnDeath?.Invoke();

        if (CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}


