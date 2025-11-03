using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private Collider attackCollider; // IsTrigger = true
    [SerializeField] private int damage = 10;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float activeTime = 0.25f;

    [Header("Filter")]
    [SerializeField] private LayerMask targetLayers; // im Inspector auf "Enemy" setzen
    [SerializeField] private bool ignoreSameRoot = true;

    private bool attacking;
    private Transform _root;

    private void Awake()
    {
        _root = transform.root;
        if (attackCollider) {
            attackCollider.enabled = false;
            attackCollider.isTrigger = true;
            // Sicherheit: lege Hitbox auf PlayerAttack-Layer
            attackCollider.gameObject.layer = LayerMask.NameToLayer("PlayerAttack");
        }
    }

    public void Attack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || attacking) return;
        StartCoroutine(DoAttack());
    }

    private System.Collections.IEnumerator DoAttack()
    {
        attacking = true;
        if (attackCollider) attackCollider.enabled = true;

        yield return new WaitForSeconds(activeTime);

        if (attackCollider) attackCollider.enabled = false;
        attacking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!attacking) return;
        if (!IsInMask(other.gameObject.layer, targetLayers)) return;       // nur Enemy
        if (ignoreSameRoot && other.transform.root == _root) return;       // nie sich selbst

        var hp = other.GetComponent<Hitpoints>() ?? other.GetComponentInParent<Hitpoints>();
        if (!hp) return;

        Vector3 dir = (other.transform.position - transform.position).normalized;
        hp.TakeDamage(damage, dir, knockbackForce);
    }

    private static bool IsInMask(int layer, LayerMask mask) => (mask.value & (1 << layer)) != 0;
}