using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    private enum State { Idle, Chase, Telegraph, Charge, HitRecover }

    [Header("Target")]
    public Transform player;

    [Header("Chase")]
    public float detectRange = 12f;
    public float moveSpeed = 5f;
    public float stopDistance = 1.25f;

    [Header("Charge")]
    public float chargeRange = 6f;
    public float chargeSpeed = 10f;
    public float telegraphTime = 0.35f;
    public float chargeDuration = 0.45f;
    public float chargeCooldown = 1.5f;

    [Header("Hit Recover (after dealing contact damage)")]
    public float afterHitTime = 0.15f;
    public float afterHitSpeed = 2.5f; // leichter Rückstoß weg vom Ziel

    [Header("Rotation")]
    public bool rotateToMoveDirection = true;

    private Rigidbody rb;

    private State state = State.Idle;
    private float stateTimer;
    private float cooldownTimer;

    private Vector3 chargeDir;
    private Vector3 hitRecoverDir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints |= RigidbodyConstraints.FreezeRotation;

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        state = State.Chase;
    }

    private void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
        if (stateTimer > 0f) stateTimer -= Time.deltaTime;

        // Wenn kein Player: stehen bleiben
        if (player == null)
        {
            SetVelocityBounded(Vector3.zero);
            return;
        }

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        float dist = toPlayer.magnitude;
        bool inDetect = dist <= detectRange;

        Vector3 dir = dist > 0.0001f ? (toPlayer / dist) : Vector3.zero;

        switch (state)
        {
            case State.Idle:
            {
                SetVelocityBounded(Vector3.zero);

                if (inDetect) state = State.Chase;
                break;
            }

            case State.Chase:
            {
                if (!inDetect)
                {
                    state = State.Idle;
                    SetVelocityBounded(Vector3.zero);
                    break;
                }

                if (dist <= stopDistance)
                {
                    SetVelocityBounded(Vector3.zero);
                }
                else
                {
                    // läuft zum Player; an der Wand wird automatisch "geslidet"
                    SetVelocityBounded(dir * moveSpeed);
                }

                // Charge starten
                if (cooldownTimer <= 0f && dist <= chargeRange && dist > stopDistance + 0.25f)
                {
                    state = State.Telegraph;
                    stateTimer = telegraphTime;
                }

                break;
            }

            case State.Telegraph:
            {
                // kurz "anspannen"
                SetVelocityBounded(Vector3.zero);

                if (stateTimer <= 0f)
                {
                    chargeDir = dir;
                    state = State.Charge;
                    stateTimer = chargeDuration;
                }

                break;
            }

            case State.Charge:
            {
                // Wenn wir beim Charge an der Arena-Grenze blocken -> sofort wieder verfolgen
                bool blocked = SetVelocityBounded(chargeDir * chargeSpeed);

                if (blocked)
                {
                    cooldownTimer = chargeCooldown;
                    state = State.Chase;
                    break;
                }

                if (stateTimer <= 0f)
                {
                    cooldownTimer = chargeCooldown;
                    state = State.Chase;
                }

                break;
            }

            case State.HitRecover:
            {
                // kurzer Rückstoß weg vom Ziel; an Arena-Grenze ebenfalls sauber begrenzt
                SetVelocityBounded(hitRecoverDir * afterHitSpeed);

                if (stateTimer <= 0f)
                {
                    state = State.Chase; // sofort wieder verfolgen
                }

                break;
            }
        }
    }

    /// <summary>
    /// Wird von ContactDamage aufgerufen, wenn der Gegner Schaden per Kontakt verursacht hat.
    /// Erwartet ContactDamage genau so. :contentReference[oaicite:1]{index=1}
    /// </summary>
    public void OnDealtDamage(Vector3 targetPosition)
    {
        // Optional: wenn du NICHT willst, dass ein Charge dadurch unterbrochen wird:
        // if (state == State.Charge || state == State.Telegraph) return;

        Vector3 away = (transform.position - targetPosition);
        away.y = 0f;

        hitRecoverDir = away.sqrMagnitude > 0.0001f ? away.normalized : Vector3.zero;

        state = State.HitRecover;
        stateTimer = afterHitTime;

        // Optional: verhindert sofortiges Re-Charge-Spam nach Kontakt
        cooldownTimer = Mathf.Max(cooldownTimer, 0.2f);
    }

    /// <summary>
    /// Setzt Velocity, aber verhindert, dass der Gegner die Arena verlässt (auch beim Dash/Charge).
    /// Gibt true zurück, wenn die gewünschte Bewegung durch die Arena-Grenze blockiert wurde.
    /// </summary>
    private bool SetVelocityBounded(Vector3 v)
    {
        v.y = 0f;

        // Ohne Arena -> normales Verhalten
        if (EnemyArenaBounds.Instance == null)
        {
            rb.linearVelocity = v;
            RotateIfNeeded(v);
            return false;
        }

        Vector3 current = rb.position;
        Vector3 desiredNext = current + v * Time.deltaTime;

        Vector3 clampedNext = EnemyArenaBounds.Instance.ClampToBounds(desiredNext);

        bool blocked = (clampedNext - desiredNext).sqrMagnitude > 0.0000001f;

        if (!blocked)
        {
            rb.linearVelocity = v;
            RotateIfNeeded(v);
            return false;
        }

        // Slide: nur die erlaubte Bewegung nehmen
        Vector3 delta = clampedNext - current;
        Vector3 correctedVel = delta / Mathf.Max(Time.deltaTime, 0.0001f);
        correctedVel.y = 0f;

        if (correctedVel.sqrMagnitude < 0.0001f)
            rb.linearVelocity = Vector3.zero;
        else
            rb.linearVelocity = correctedVel;

        RotateIfNeeded(rb.linearVelocity);
        return true;
    }

    private void RotateIfNeeded(Vector3 vel)
    {
        if (!rotateToMoveDirection) return;

        Vector3 flat = vel;
        flat.y = 0f;

        if (flat.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(flat, Vector3.up);
    }
}



