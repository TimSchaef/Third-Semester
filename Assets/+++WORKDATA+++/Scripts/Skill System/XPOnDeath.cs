using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class XPOnDeath : MonoBehaviour
{
    [Header("XP Settings")]
    public int baseXP = 10;       // Grund-XP, die dieser Gegner gibt

    [Header("Refs")]
    public XPGiver xpGiver;       // Kann im Inspector gesetzt werden, wird sonst automatisch gesucht

    private HealthComponent health;

    private void Awake()
    {
        health = GetComponent<HealthComponent>();

        if (health != null)
            health.OnDeath += HandleDeath;

        // Falls kein xpGiver im Inspector gesetzt ist: automatisch Player suchen
        if (xpGiver == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                xpGiver = player.GetComponent<XPGiver>();
        }
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (xpGiver != null)
        {
            xpGiver.GrantXP(baseXP);
        }
        else
        {
            Debug.LogWarning($"{name}: XPOnDeath hat keinen XPGiver gefunden.");
        }
    }
}



