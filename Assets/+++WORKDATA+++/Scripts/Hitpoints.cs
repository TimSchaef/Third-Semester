using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class Hitpoints : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private PlayerStatsManager stats; // Für Armor, HP, Regen, Thorns etc.
    [SerializeField] private bool useStatsForMaxHP = true;

    [Header("Health")]
    [SerializeField] private int maxHitPoints = 100;
    [SerializeField] private int hitPoints = 100;
    [SerializeField] private UIHitPoints uiHitpoints;
    [SerializeField] private EnemyHealthBar enemyHealthBar;

    [Header("Death / XP")]
    [SerializeField] private bool isPlayer = false;
    [SerializeField] private int xpOnDeath = 10; // nur bei Gegner relevant

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (useStatsForMaxHP && stats != null)
        {
            int fromStat = Mathf.RoundToInt(stats.GetValue(CoreStatId.MaxHP));
            if (fromStat > 0) maxHitPoints = fromStat;
        }

        maxHitPoints = Mathf.Max(1, maxHitPoints);
        hitPoints = Mathf.Clamp(hitPoints, 0, maxHitPoints);

        if (uiHitpoints) uiHitpoints.UpdateHitpoints(hitPoints);
        if (enemyHealthBar) enemyHealthBar.Init(transform, maxHitPoints, hitPoints);
    }

    private void Update()
    {
        if (stats != null && hitPoints > 0 && hitPoints < maxHitPoints)
        {
            float regen = Mathf.Max(0f, stats.GetValue(CoreStatId.HPRegen));
            if (regen > 0f)
            {
                hitPoints = Mathf.Min(maxHitPoints, hitPoints + Mathf.RoundToInt(regen * Time.deltaTime));
                if (uiHitpoints) uiHitpoints.UpdateHitpoints(hitPoints);
                if (enemyHealthBar) enemyHealthBar.Set(hitPoints);
            }
        }
    }

    /// <summary>
    /// Schaden anwenden inkl. Armor, Thorns, LifeSteal, XPGain, CameraShake usw.
    /// </summary>
    public float TakeDamage(
        int rawDamage,
        Vector3 knockbackDirection,
        float knockbackForce,
        PlayerStatsManager attackerStats = null,
        Hitpoints attackerHealth = null)
    {
        if (hitPoints <= 0) return 0f;

        // Armor-Reduktion (weiche Kurve)
        float armor = stats ? Mathf.Max(0f, stats.GetValue(CoreStatId.Armor)) : 0f;
        float reduction = armor / (armor + 100f);
        float taken = Mathf.Max(0f, rawDamage * (1f - reduction));

        int takenInt = Mathf.RoundToInt(taken);
        hitPoints = Mathf.Clamp(hitPoints - takenInt, 0, maxHitPoints);

        // Knockback
        if (rb)
        {
            knockbackDirection.y = 0f;
            rb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode.Impulse);
        }

        // Enemy AI Reaction
        var enemy = GetComponent<CotLStyleEnemy3D>();
        if (enemy) enemy.Hit(knockbackDirection * knockbackForce, 0.25f);

        // UI
        if (uiHitpoints) uiHitpoints.UpdateHitpoints(hitPoints);
        if (enemyHealthBar) enemyHealthBar.Set(hitPoints);

        // Damage Number
        if (DamageNumberSpawner.Instance)
            DamageNumberSpawner.Instance.SpawnDamage(takenInt, transform);

        // Thorns
        if (stats != null && attackerHealth != null && attackerHealth.hitPoints > 0)
        {
            float thorns = Mathf.Clamp01(stats.GetValue(CoreStatId.Thorns));
            if (thorns > 0f)
            {
                int reflect = Mathf.RoundToInt(takenInt * thorns);
                attackerHealth.ApplyPureDamage(reflect, showCameraShake: false);

                if (reflect > 0 && DamageNumberSpawner.Instance)
                    DamageNumberSpawner.Instance.SpawnThorns(reflect, attackerHealth.transform);
            }
        }

        // LifeSteal
        if (attackerStats != null && attackerHealth != null && takenInt > 0)
        {
            float ls = Mathf.Clamp01(attackerStats.GetValue(CoreStatId.LifeSteal));
            if (ls > 0f)
            {
                int heal = Mathf.RoundToInt(takenInt * ls);
                attackerHealth.Heal(heal);
                if (heal > 0 && DamageNumberSpawner.Instance)
                    DamageNumberSpawner.Instance.SpawnHeal(heal, attackerHealth.transform);
            }
        }

        // Death
        if (hitPoints <= 0)
        {
            OnDeath(attackerStats);
        }

        // Camera Shake on Player hit
        if (isPlayer && Camera.main)
            Camera.main.DOShakePosition(0.5f, new Vector3(0.2f, 0.2f, 0));

        return takenInt;
    }

    public void ApplyPureDamage(int amount, bool showCameraShake = true)
    {
        if (hitPoints <= 0) return;

        hitPoints = Mathf.Clamp(hitPoints - Mathf.Max(0, amount), 0, maxHitPoints);

        if (uiHitpoints) uiHitpoints.UpdateHitpoints(hitPoints);
        if (enemyHealthBar) enemyHealthBar.Set(hitPoints);

        if (amount > 0 && DamageNumberSpawner.Instance)
            DamageNumberSpawner.Instance.SpawnDamage(amount, transform);

        if (hitPoints <= 0) OnDeath(null);

        if (showCameraShake && isPlayer && Camera.main)
            Camera.main.DOShakePosition(0.5f, new Vector3(0.2f, 0.2f, 0));
    }

    public void Heal(int amount)
    {
        if (hitPoints <= 0) return;
        hitPoints = Mathf.Clamp(hitPoints + Mathf.Max(0, amount), 0, maxHitPoints);

        if (uiHitpoints) uiHitpoints.UpdateHitpoints(hitPoints);
        if (enemyHealthBar) enemyHealthBar.Set(hitPoints);

        if (amount > 0 && DamageNumberSpawner.Instance)
            DamageNumberSpawner.Instance.SpawnHeal(amount, transform);
    }

    private void OnDeath(PlayerStatsManager killerStats)
    {
        if (isPlayer)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            if (killerStats && killerStats.player)
            {
                float mult = Mathf.Max(0f, killerStats.GetValue(CoreStatId.XPGain)); // 1.0 = normal
                killerStats.player.AddXPMultiplied(xpOnDeath, mult);
            }
            Destroy(gameObject);
        }
    }
}





