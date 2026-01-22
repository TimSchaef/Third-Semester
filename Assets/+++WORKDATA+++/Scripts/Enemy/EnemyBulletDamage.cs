using UnityEngine;

public class EnemyBulletDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damage = 10f;

    [Header("Filter")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Optional: Wenn gesetzt, wird nur Schaden gemacht, wenn das getroffene Objekt in diesen Layern liegt.")]
    [SerializeField] private LayerMask playerLayers = ~0;

    [Header("Lifetime")]
    [SerializeField] private bool destroyOnHitPlayer = true;
    [SerializeField] private bool destroyOnHitWorld = false;

    // Optional: Owner-Schutz
    private Transform ownerRoot;
    public void InitOwner(Transform owner) => ownerRoot = owner;

    private void HandleHit(Collider other)
    {
        // Owner ignorieren
        if (ownerRoot != null && other.transform.IsChildOf(ownerRoot)) return;

        // Layer-Filter (optional)
        if (((1 << other.gameObject.layer) & playerLayers) == 0) return;

        // Nur Player
        if (!other.CompareTag(playerTag)) return;

        // HealthComponent auf dem Player (Parent-tolerant)
        var hc = other.GetComponentInParent<HealthComponent>();
        if (hc == null)
        {
            // Wenn hier null: HealthComponent ist nicht am Parent-Tree des Colliders.
            // Dann sitzt es woanders oder Tag ist auf falschem Objekt.
            return;
        }

        hc.ApplyDamage(damage, attacker: null); // :contentReference[oaicite:1]{index=1}

        if (destroyOnHitPlayer)
            Destroy(gameObject);
    }

    // Für Trigger-Collider
    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    // Für normale physikalische Kollisionen
    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.collider);

        if (destroyOnHitWorld && !collision.collider.CompareTag(playerTag))
            Destroy(gameObject);
    }
}



