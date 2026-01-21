using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class XPOnDeath : MonoBehaviour
{
    [Header("XP Settings")]
    public int baseXP = 10;

    [Header("Drop Settings")]
    public GameObject xpDropPrefab;       // Prefab mit XPPickup + Trigger Collider
    public Vector3 dropOffset = new Vector3(0, 0.5f, 0);

    private HealthComponent health;

    private void Awake()
    {
        health = GetComponent<HealthComponent>();
        if (health != null)
            health.OnDeath += HandleDeath;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (xpDropPrefab == null)
        {
            Debug.LogWarning($"{name}: XPOnDeath hat kein xpDropPrefab gesetzt.");
            return;
        }

        // Drop spawnen
        var go = Instantiate(xpDropPrefab, transform.position + dropOffset, Quaternion.identity);

        // XP-Wert setzen
        var pickup = go.GetComponent<XPPickup>();
        if (pickup != null)
            pickup.xpAmount = baseXP;
        else
            Debug.LogWarning($"{name}: xpDropPrefab hat kein XPPickup Script.");
    }
}



