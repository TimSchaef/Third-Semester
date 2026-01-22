using UnityEngine;

public class EnemyProjectileShooter : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Wenn leer, wird per Player-Tag gesucht.")]
    [SerializeField] private Transform targetRoot;

    [Tooltip("Optionaler Child-Name am Player, der wirklich mitläuft (z.B. 'Player', 'Model', 'Center', 'AimPoint').")]
    [SerializeField] private string targetChildName = "AimPoint";

    [SerializeField] private bool aimFlatIgnoreY = false;

    [Header("Projectile")]
    [SerializeField] private Rigidbody projectilePrefab;
    [SerializeField] private Transform muzzle;

    [Header("Fire")]
    [SerializeField] private float fireCooldown = 1.0f;
    [SerializeField] private float projectileSpeed = 12f;

    [Header("Spawn Offsets")]
    [SerializeField] private float spawnForwardOffset = 0.6f;
    [SerializeField] private float spawnUpOffset = 0.2f;

    [Header("Ground Offset (Raycast)")]
    [SerializeField] private bool snapToGround = true;
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float groundRayHeight = 3f;
    [SerializeField] private float groundRayDistance = 10f;
    [SerializeField] private float groundOffset = 0.15f;

    private float _nextFireTime;

    private void Reset()
    {
        muzzle = transform;
    }

    private void Update()
    {
        if (!projectilePrefab) return;

        if (Time.time >= _nextFireTime)
        {
            FireAtCurrentTargetPosition();
            _nextFireTime = Time.time + fireCooldown;
        }
    }

    public void FireAtCurrentTargetPosition()
    {
        Transform target = ResolveTargetTransform();
        if (!target) return;

        Transform spawnT = muzzle ? muzzle : transform;

        Vector3 spawnPos = spawnT.position
                         + spawnT.forward * spawnForwardOffset
                         + Vector3.up * spawnUpOffset;

        if (snapToGround)
        {
            Vector3 rayStart = spawnPos + Vector3.up * groundRayHeight;
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundRayDistance, groundLayers, QueryTriggerInteraction.Ignore))
            {
                spawnPos.y = hit.point.y + groundOffset;
            }
        }

        // IMMER aktuelle Zielposition zur Schusszeit:
        Vector3 targetPos = target.position;

        if (aimFlatIgnoreY)
            targetPos.y = spawnPos.y;

        Vector3 dir = targetPos - spawnPos;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        Rigidbody rb = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir));

        // Wichtig: velocity (nicht linearVelocity), damit es in den meisten Unity-Versionen korrekt ist
        rb.linearVelocity = dir * projectileSpeed;
    }

    private Transform ResolveTargetTransform()
    {
        // 1) Root holen/validieren
        if (!targetRoot)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go) targetRoot = go.transform;
        }

        if (!targetRoot) return null;

        // 2) Wenn ein Child-Name angegeben ist: den mitlaufenden Transform nutzen
        if (!string.IsNullOrWhiteSpace(targetChildName))
        {
            Transform child = targetRoot.Find(targetChildName);
            if (child) return child;
        }

        // Fallback: Root selbst
        return targetRoot;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Transform t = ResolveTargetTransform();
        if (!t) return;

        Gizmos.DrawLine(transform.position, t.position);
    }
#endif
}



