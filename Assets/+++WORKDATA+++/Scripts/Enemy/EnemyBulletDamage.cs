using UnityEngine;

public class EnemyBulletDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damage = 10f;

    [Header("Filter")]
    [SerializeField] private string playerTag = "Player";
    
    [SerializeField] private LayerMask playerLayers = ~0;

    [Header("Lifetime")]
    [SerializeField] private bool destroyOnHitPlayer = true;
    [SerializeField] private bool destroyOnHitWorld = false;
    
    private Transform ownerRoot;
    public void InitOwner(Transform owner) => ownerRoot = owner;

    private void HandleHit(Collider other)
    {
        if (ownerRoot != null && other.transform.IsChildOf(ownerRoot)) return;
        
        if (((1 << other.gameObject.layer) & playerLayers) == 0) return;
        
        if (!other.CompareTag(playerTag)) return;
        
        var hc = other.GetComponentInParent<HealthComponent>();
        if (hc == null)
        {
            return;
        }

        hc.ApplyDamage(damage, attacker: null); 

        if (destroyOnHitPlayer)
            Destroy(gameObject);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.collider);

        if (destroyOnHitWorld && !collision.collider.CompareTag(playerTag))
            Destroy(gameObject);
    }
}



