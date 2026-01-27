using UnityEngine;

public class EnemyProjectileShooter : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform targetRoot;
    
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

    [Header("Ground Offset")]
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
        
        Vector3 targetPos = target.position;

        if (aimFlatIgnoreY)
            targetPos.y = spawnPos.y;

        Vector3 dir = targetPos - spawnPos;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        Rigidbody rb = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir));
        
        rb.linearVelocity = dir * projectileSpeed;
    }

    private Transform ResolveTargetTransform()
    {
        if (!targetRoot)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go) targetRoot = go.transform;
        }

        if (!targetRoot) return null;

        
        if (!string.IsNullOrWhiteSpace(targetChildName))
        {
            Transform child = targetRoot.Find(targetChildName);
            if (child) return child;
        }

        
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



