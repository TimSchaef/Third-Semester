using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ContactDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float damageInterval = 1f; // alle X Sekunden bei dauerhaftem Kontakt

    [Header("Collision Mode")]
    [SerializeField] private bool useTrigger = true;
    [SerializeField] private LayerMask targetLayers;   // z.B. Player
    [SerializeField] private bool ignoreSameRoot = true;

    private Transform _root;
    private HealthComponent _myHealth;

    // Für Dauer-Schaden: wie lange bis zum nächsten Tick pro Collider
    private readonly Dictionary<Collider, float> _nextDamageTime = new Dictionary<Collider, float>();

    private void Awake()
    {
        _root = transform.root;
        _myHealth = GetComponentInParent<HealthComponent>();

        var col = GetComponent<Collider>();
        if (col) col.isTrigger = useTrigger;
    }

    // ---------- TRIGGER VARIANTE ----------

    private void OnTriggerEnter(Collider other)
    {
        if (!useTrigger) return;
        HandleEnter(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!useTrigger) return;
        HandleStay(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!useTrigger) return;
        HandleExit(other);
    }

    // ---------- COLLISION VARIANTE ----------

    private void OnCollisionEnter(Collision collision)
    {
        if (useTrigger) return;
        HandleEnter(collision.collider);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (useTrigger) return;
        HandleStay(collision.collider);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (useTrigger) return;
        HandleExit(collision.collider);
    }

    // ---------- LOGIK ----------

    private void HandleEnter(Collider other)
    {
        if (!IsValidTarget(other)) return;

        // Sofort-Schaden beim ersten Kontakt
        ApplyDamage(other);

        // Nächster Tick in damageInterval Sekunden
        if (damageInterval > 0f)
        {
            _nextDamageTime[other] = Time.time + damageInterval;
        }
    }

    private void HandleStay(Collider other)
    {
        if (!IsValidTarget(other)) return;
        if (damageInterval <= 0f) return;

        if (!_nextDamageTime.TryGetValue(other, out float nextTime))
        {
            _nextDamageTime[other] = Time.time + damageInterval;
            return;
        }

        if (Time.time >= nextTime)
        {
            ApplyDamage(other);
            _nextDamageTime[other] = Time.time + damageInterval;
        }
    }

    private void HandleExit(Collider other)
    {
        if (_nextDamageTime.ContainsKey(other))
        {
            _nextDamageTime.Remove(other);
        }
    }

    private bool IsValidTarget(Collider other)
    {
        if (!IsInMask(other.gameObject.layer, targetLayers)) return false;
        if (ignoreSameRoot && other.transform.root == _root) return false;
        return true;
    }

    private void ApplyDamage(Collider other)
    {
        var targetHp = other.GetComponent<HealthComponent>() ?? other.GetComponentInParent<HealthComponent>();
        if (!targetHp) return;

        Vector3 dir = (other.transform.position - transform.position).normalized;

        targetHp.ApplyDamage(damage, _myHealth);
        
        
        var enemyMove = GetComponentInParent<EnemyMovement>();
        if (enemyMove != null)
        {
            enemyMove.OnDealtDamage(other.transform.position);
        }

        var rb = other.attachedRigidbody;
        if (rb != null)
        {
            dir.y = 0f;
            rb.AddForce(dir.normalized * knockbackForce, ForceMode.Impulse);
        }
    }


    private static bool IsInMask(int layer, LayerMask mask) => (mask.value & (1 << layer)) != 0;
}


