using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    private enum State { Idle, Chase, Telegraph, Charge, HitRecover } // ADDED

    [Header("Target")]
    public Transform player;

    [Header("Basic Movement")]
    public float detectRange = 12f;
    public float moveSpeed = 5f;

    [Header("Charge")]
    public float chargeRange = 5f;
    public float telegraphTime = 0.35f;
    public float chargeSpeed = 11f;
    public float chargeDuration = 0.45f;
    public float chargeCooldown = 1.2f;

    [Header("After Dealing Damage")]                 // ADDED
    public bool retreatAfterHit = true;              // ADDED (false => just stop)
    public float afterHitTime = 0.35f;               // ADDED
    public float retreatSpeed = 6f;                  // ADDED

    private Rigidbody rb;
    private State state = State.Idle;

    private float stateTimer = 0f;
    private float cooldownTimer = 0f;

    private Vector3 chargeDir = Vector3.zero;

    private Vector3 hitRecoverDir = Vector3.zero;    // ADDED

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY |
                         RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        if (!player)
        {
            SetVelocity(Vector3.zero);
            state = State.Idle;
            return;
        }

        cooldownTimer -= Time.deltaTime;
        stateTimer -= Time.deltaTime;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        float dist = toPlayer.magnitude;
        Vector3 dir = dist > 0.0001f ? (toPlayer / dist) : Vector3.zero;

        switch (state)
        {
            case State.HitRecover: // ADDED
                if (retreatAfterHit)
                    SetVelocity(hitRecoverDir * retreatSpeed);
                else
                    SetVelocity(Vector3.zero);

                if (stateTimer <= 0f)
                    state = State.Chase;
                break;

            case State.Idle:
                SetVelocity(Vector3.zero);

                if (dist <= detectRange)
                    state = State.Chase;
                break;

            case State.Chase:
                if (dist <= chargeRange && cooldownTimer <= 0f)
                {
                    SetVelocity(Vector3.zero);
                    state = State.Telegraph;
                    stateTimer = telegraphTime;
                    break;
                }

                SetVelocity(dir * moveSpeed);

                if (dist > detectRange * 1.2f)
                {
                    SetVelocity(Vector3.zero);
                    state = State.Idle;
                }
                break;

            case State.Telegraph:
                SetVelocity(Vector3.zero);

                if (stateTimer <= 0f)
                {
                    chargeDir = dir;

                    state = State.Charge;
                    stateTimer = chargeDuration;

                    SetVelocity(chargeDir * chargeSpeed);
                }
                break;

            case State.Charge:
                SetVelocity(chargeDir * chargeSpeed);

                if (stateTimer <= 0f)
                {
                    cooldownTimer = chargeCooldown;
                    state = State.Chase;
                }
                break;
        }
    }

    // ADDED: called by ContactDamage when damage is dealt
    public void OnDealtDamage(Vector3 targetPosition)
    {
        // If you want to prevent interrupting charge/telegraph, uncomment:
        // if (state == State.Charge || state == State.Telegraph) return;

        Vector3 away = (transform.position - targetPosition);
        away.y = 0f;

        hitRecoverDir = away.sqrMagnitude > 0.0001f ? away.normalized : Vector3.zero;

        state = State.HitRecover;
        stateTimer = afterHitTime;

        // Optional: also start cooldown so it doesn't instantly re-charge
        // cooldownTimer = Mathf.Max(cooldownTimer, 0.2f);
    }

    private void SetVelocity(Vector3 v)
    {
        v.y = 0f;
        rb.linearVelocity = v;
    }
}


