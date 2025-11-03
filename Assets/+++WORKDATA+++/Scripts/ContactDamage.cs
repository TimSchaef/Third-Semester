using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ContactDamage : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private bool useTrigger = true;
    [SerializeField] private LayerMask targetLayers;   // z.B. Player
    [SerializeField] private bool ignoreSameRoot = true;

    private Transform _root;

    private void Awake()
    {
        _root = transform.root;
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = useTrigger;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useTrigger) return;
        TryDamage(other);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (useTrigger) return;
        TryDamage(other.collider);
    }

    private void TryDamage(Collider other)
    {
        if (!IsInMask(other.gameObject.layer, targetLayers)) return;
        if (ignoreSameRoot && other.transform.root == _root) return;

        var hp = other.GetComponent<Hitpoints>() ?? other.GetComponentInParent<Hitpoints>();
        if (!hp) return;

        Vector3 dir = (other.transform.position - transform.position).normalized;
        hp.TakeDamage(damage, dir, knockbackForce);
    }

    private static bool IsInMask(int layer, LayerMask mask) => (mask.value & (1 << layer)) != 0;
}