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
    public float chaseAcceleration = 20f;
    public float minChaseSpeedAfterKnockback = 0.75f;

    [Header("Charge")]
    public float chargeRange = 6f;
    public float chargeSpeed = 10f;
    public float telegraphTime = 0.35f;
    public float chargeDuration = 0.45f;
    public float chargeCooldown = 1.5f;

    [Header("Knockback")]
    public float knockbackDeceleration = 25f;
    public float knockbackStopHoldTime = 0.05f;

    [Header("Hit Recover")]
    public float afterHitTime = 0.15f;
    public float afterHitSpeed = 2.5f;

    [Header("Rotation")]
    public bool rotateToMoveDirection = true;

    [Header("Hover")]
    public bool enableIdleHover = false;
    public Transform hoverVisualTransform;
    public float hoverHeight = 0.5f;
    public float hoverSpeed = 1.5f;
    public bool hoverOnlyInIdleAndChase = true;

    private Vector3 hoverVisualLocalStartPos;

    private Rigidbody rb;

    private State state = State.Idle;
    private float stateTimer;
    private float cooldownTimer;

    private Vector3 chargeDir;

    private Vector3 hitRecoverDir;
    private float currentKnockbackSpeed = 0f;
    private bool inKnockbackStopPhase = false;

    private float currentChaseSpeed = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints |= RigidbodyConstraints.FreezeRotation;

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (enableIdleHover && hoverVisualTransform == null)
        {
            var visual = transform.Find("Visual");
            if (visual != null) hoverVisualTransform = visual;

            if (hoverVisualTransform == null)
            {
                var sr = GetComponentInChildren<SpriteRenderer>(true);
                if (sr != null) hoverVisualTransform = sr.transform;
                else
                {
                    var r = GetComponentInChildren<Renderer>(true);
                    if (r != null) hoverVisualTransform = r.transform;
                }
            }
        }

        if (hoverVisualTransform != null)
            hoverVisualLocalStartPos = hoverVisualTransform.localPosition;

        state = State.Chase;
        currentChaseSpeed = 0f;
    }

    private void Update()
    {
        if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
        if (stateTimer > 0f) stateTimer -= Time.deltaTime;

        // ✅ NEU: Wenn der Gegner außerhalb der Area ist -> anhalten + zurück in Bounds
        if (EnemyAreaBounds.Instance != null && !EnemyAreaBounds.Instance.Contains(rb.position))
        {
            Vector3 clamped = EnemyAreaBounds.Instance.ClampToBounds(rb.position);

            // Snap zurück in die Area
            rb.position = clamped;

            // hart stoppen
            rb.linearVelocity = Vector3.zero;

            // Zustände resetten, damit er danach wieder normal chased
            state = State.Chase;
            stateTimer = 0f;
            inKnockbackStopPhase = false;
            currentKnockbackSpeed = 0f;

            return; // nächste Frames läuft Chase wieder ganz normal
        }

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
                currentChaseSpeed = 0f;

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
                    currentChaseSpeed = Mathf.MoveTowards(currentChaseSpeed, 0f, chaseAcceleration * Time.deltaTime);
                }
                else
                {
                    float accel = Mathf.Max(0.01f, chaseAcceleration);
                    currentChaseSpeed = Mathf.MoveTowards(currentChaseSpeed, moveSpeed, accel * Time.deltaTime);
                    SetVelocityBounded(dir * currentChaseSpeed);
                }

                if (cooldownTimer <= 0f && dist <= chargeRange && dist > stopDistance + 0.25f)
                {
                    state = State.Telegraph;
                    stateTimer = telegraphTime;
                }

                break;
            }

            case State.Telegraph:
            {
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
                bool blocked = SetVelocityBounded(chargeDir * chargeSpeed);

                if (blocked)
                {
                    cooldownTimer = chargeCooldown;
                    state = State.Chase;
                    currentChaseSpeed = Mathf.Min(currentChaseSpeed, moveSpeed * 0.6f);
                    break;
                }

                if (stateTimer <= 0f)
                {
                    cooldownTimer = chargeCooldown;
                    state = State.Chase;
                    currentChaseSpeed = Mathf.Min(currentChaseSpeed, moveSpeed * 0.8f);
                }

                break;
            }

            case State.HitRecover:
            {
                if (!inKnockbackStopPhase)
                {
                    float decel = Mathf.Max(0.01f, knockbackDeceleration);
                    currentKnockbackSpeed = Mathf.MoveTowards(currentKnockbackSpeed, 0f, decel * Time.deltaTime);

                    if (currentKnockbackSpeed > 0.0001f)
                    {
                        SetVelocityBounded(hitRecoverDir * currentKnockbackSpeed);
                    }
                    else
                    {
                        SetVelocityBounded(Vector3.zero);
                        inKnockbackStopPhase = true;
                        stateTimer = Mathf.Max(0f, knockbackStopHoldTime);
                    }
                }
                else
                {
                    SetVelocityBounded(Vector3.zero);

                    if (stateTimer <= 0f)
                    {
                        state = State.Chase;
                        float minAfter = Mathf.Clamp(minChaseSpeedAfterKnockback, 0f, moveSpeed);
                        currentChaseSpeed = Mathf.Min(currentChaseSpeed, minAfter);

                        inKnockbackStopPhase = false;
                    }
                }

                break;
            }
        }
    }

    private void LateUpdate()
    {
        if (!enableIdleHover || hoverVisualTransform == null) return;

        if (player == null)
        {
            hoverVisualTransform.localPosition = hoverVisualLocalStartPos;
            return;
        }

        if (hoverOnlyInIdleAndChase && state != State.Idle && state != State.Chase)
        {
            hoverVisualTransform.localPosition = hoverVisualLocalStartPos;
            return;
        }

        float yOffset = Mathf.PingPong(Time.time * hoverSpeed, hoverHeight);
        hoverVisualTransform.localPosition = hoverVisualLocalStartPos + Vector3.up * yOffset;
    }

    public void OnDealtDamage(Vector3 targetPosition)
    {
        Vector3 away = (transform.position - targetPosition);
        away.y = 0f;

        if (away.sqrMagnitude < 0.0001f)
            away = transform.forward;

        BeginKnockback(away.normalized, afterHitSpeed, afterHitTime);
        cooldownTimer = Mathf.Max(cooldownTimer, 0.2f);
    }

    public void ApplyKnockback(Vector3 sourcePosition, float force, float duration = 0.15f, float minForce = 0f)
    {
        Vector3 away = (transform.position - sourcePosition);
        away.y = 0f;

        if (away.sqrMagnitude < 0.0001f)
            away = transform.forward;

        float startSpeed = Mathf.Max(minForce, force);
        BeginKnockback(away.normalized, startSpeed, duration);

        cooldownTimer = Mathf.Max(cooldownTimer, 0.2f);
    }

    private void BeginKnockback(Vector3 dir, float startSpeed, float minDuration)
    {
        hitRecoverDir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.zero;

        state = State.HitRecover;
        inKnockbackStopPhase = false;

        currentKnockbackSpeed = Mathf.Max(0f, startSpeed);
        stateTimer = Mathf.Max(0f, minDuration);

        currentChaseSpeed = Mathf.Min(currentChaseSpeed, minChaseSpeedAfterKnockback);
    }

    private bool SetVelocityBounded(Vector3 v)
    {
        v.y = 0f;

        if (EnemyAreaBounds.Instance == null)
        {
            rb.linearVelocity = v;
            RotateIfNeeded(v);
            return false;
        }

        Vector3 current = rb.position;
        Vector3 desiredNext = current + v * Time.deltaTime;

        Vector3 clampedNext = EnemyAreaBounds.Instance.ClampToBounds(desiredNext);

        bool blocked = (clampedNext - desiredNext).sqrMagnitude > 0.0000001f;

        if (!blocked)
        {
            rb.linearVelocity = v;
            RotateIfNeeded(v);
            return false;
        }

        Vector3 delta = clampedNext - current;
        Vector3 correctedVel = delta / Mathf.Max(Time.deltaTime, 0.0001f);
        correctedVel.y = 0f;

        rb.linearVelocity = (correctedVel.sqrMagnitude < 0.0001f) ? Vector3.zero : correctedVel;

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

