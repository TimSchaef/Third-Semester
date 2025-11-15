using UnityEngine;

public class AttackController : MonoBehaviour
{
    public PlayerStatsManager stats;
    public HealthComponent selfHealth;
    public float baseAttackCooldown = 1.0f; // Sekunden
    public float baseDamage = 20f;

    float cooldownTimer = 0f;

    void Update()
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f && Input.GetButtonDown("Fire1"))
        {
            TryAttack();
        }
    }

    void TryAttack()
    {
        float atkSpeedMult = Mathf.Max(0.01f, stats.GetValue(CoreStatId.AttackSpeed)); // 1.0 = normal
        float cd = baseAttackCooldown / atkSpeedMult;
        cooldownTimer = cd;

        // Ziel finden (Dummy – ersetze durch dein Targeting)
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f))
        {
            var target = hit.collider.GetComponentInParent<HealthComponent>();
            if (target != null && target != selfHealth)
            {
                float dealt = target.ApplyDamage(baseDamage, selfHealth);

                // Life Steal
                float ls = Mathf.Clamp01(stats.GetValue(CoreStatId.LifeSteal));
                if (ls > 0f && dealt > 0f && selfHealth != null)
                    selfHealth.Heal(dealt * ls);
            }
        }
    }
}

