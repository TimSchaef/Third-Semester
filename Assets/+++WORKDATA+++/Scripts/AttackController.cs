using UnityEngine;

public class AttackController : MonoBehaviour
{
    public PlayerStatsManager stats;
    public HealthComponent selfHealth;

    [Header("Attack")]
    public float baseAttackCooldown = 1.0f;
    public float baseDamage = 20f;
    public float attackRange = 3f;
    public float attackRadius = 1f;

    float cooldownTimer = 0f;

    // speichern der letzten Angriffsausrichtung
    Vector3 attackDirection = Vector3.forward;

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        // ► ARROW KEY ATTACK DIRECTION (NICHT Movement!)
        UpdateAttackDirection();

        if (cooldownTimer <= 0f && Input.GetButtonDown("Fire1"))
        {
            TryAttack();
        }
    }

    void UpdateAttackDirection()
    {
        // Arrow keys: "Horizontal2" und "Vertical2" selbst definieren
        // Oder alternativ direkt Input.GetKey()
        
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.UpArrow))    v = 1f;
        if (Input.GetKey(KeyCode.DownArrow))  v = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) h = 1f;
        if (Input.GetKey(KeyCode.LeftArrow))  h = -1f;

        Vector3 newDir = new Vector3(h, 0f, v);

        if (newDir.sqrMagnitude > 0.01f)
            attackDirection = newDir.normalized;   // nur aktualisieren wenn gedrückt
    }

    void TryAttack()
    {
        // Attack Speed
        float atkSpeedMult = Mathf.Max(0.01f, stats.GetValue(CoreStatId.AttackSpeed));
        cooldownTimer = baseAttackCooldown / atkSpeedMult;

        // Raycast/Spherecast in Richtung der Pfeiltasten
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.SphereCast(origin, attackRadius, attackDirection, out RaycastHit hit, attackRange))
        {
            var target = hit.collider.GetComponentInParent<HealthComponent>();
            if (target != null && target != selfHealth)
            {
                float dealt = target.ApplyDamage(baseDamage, selfHealth);

                // Life Steal
                float ls = Mathf.Clamp01(stats.GetValue(CoreStatId.LifeSteal));
                if (ls > 0f && dealt > 0f)
                    selfHealth?.Heal(dealt * ls);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + attackDirection * attackRange);
    }
}



