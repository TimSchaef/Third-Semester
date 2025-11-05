using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CotLStyleEnemy3D : MonoBehaviour
{
    public enum State { Idle, Chase, Strafe, Telegraph, Charge, Retreat, Stunned }

    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float maxSpeed = 5f;
    public float acceleration = 20f;
    public float deceleration = 30f;
    public float strafeSpeed = 4f;
    public float strafeCurvature = 2f;

    [Header("Ranges")]
    public float detectRange = 12f;
    public float strafeRange = 4.5f;
    public float chargeTriggerRange = 5f;
    public float retreatRange = 1.4f;

    [Header("Charge Attack")]
    public float telegraphTime = 0.35f;
    public float chargeSpeed = 11f;
    public float chargeDuration = 0.45f;
    public float chargeCooldown = 1.2f;
    public float chargeTurnAssist = 0.15f;

    [Header("Avoidance")]
    public float separationRadius = 1.2f;
    public float separationForce = 8f;
    public LayerMask enemyMask;

    [Header("FX (optional)")]
    public SpriteRenderer sprite;
    public Color telegraphColor = new Color(1f, 0.75f, 0.2f, 1f);
    public Color normalColor = Color.white;
    public Animator animator;

    [Header("Debug")]
    public State state = State.Idle;
    public bool drawGizmos = true;

    private Rigidbody rb;
    private Vector3 desiredVel;
    private Vector3 chargeDir;
    private float stateTimer;
    private float cooldownTimer;
    private float stunnedTimer;
    private int strafeDir = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY |
                         RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;

        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>();
        ResetVisuals();
        strafeDir = Random.value < 0.5f ? 1 : -1;
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;
        cooldownTimer -= Time.deltaTime;

        if (stunnedTimer > 0f)
        {
            stunnedTimer -= Time.deltaTime;
            if (stunnedTimer <= 0f) ChangeState(State.Chase);
            UpdateAnimator();
            return;
        }

        if (!player)
        {
            desiredVel = Vector3.zero;
            ChangeState(State.Idle);
            UpdateAnimator();
            return;
        }

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;

        switch (state)
        {
            case State.Idle:
                desiredVel = Vector3.zero;
                if (dist <= detectRange) ChangeState(State.Chase);
                break;

            case State.Chase:
                if (dist > strafeRange * 1.15f)
                    desiredVel = toPlayer.normalized * maxSpeed;
                else
                    ChangeState(State.Strafe);

                if (dist <= chargeTriggerRange && cooldownTimer <= 0f)
                    ChangeState(State.Telegraph);
                break;

            case State.Strafe:
                Vector3 dir = toPlayer.normalized;
                Vector3 tangent = new Vector3(-dir.z, 0f, dir.x) * strafeDir;
                float radialError = (dist - strafeRange);
                Vector3 corrective = -dir * radialError * strafeCurvature;
                desiredVel = tangent * strafeSpeed + corrective;

                if (dist <= retreatRange)
                    ChangeState(State.Retreat);
                else if (dist > strafeRange * 1.6f)
                    ChangeState(State.Chase);

                if (dist <= chargeTriggerRange && cooldownTimer <= 0f)
                    ChangeState(State.Telegraph);
                break;

            case State.Retreat:
                desiredVel = (-toPlayer).normalized * maxSpeed;
                if (dist > strafeRange * 0.9f) ChangeState(State.Strafe);
                break;

            case State.Telegraph:
                FaceTowards(toPlayer);
                desiredVel = Vector3.zero;
                if (stateTimer <= 0f)
                {
                    chargeDir = toPlayer.normalized;
                    ChangeState(State.Charge);
                }
                break;

            case State.Charge:
                chargeDir = Vector3.Lerp(chargeDir, toPlayer.normalized, chargeTurnAssist).normalized;
                desiredVel = chargeDir * chargeSpeed;
                if (stateTimer <= 0f)
                {
                    cooldownTimer = chargeCooldown;
                    ChangeState(State.Retreat);
                }
                break;

            case State.Stunned:
                desiredVel = Vector3.zero;
                break;
        }

        UpdateAnimator();
    }

    void FixedUpdate()
    {
        Vector3 targetVel = desiredVel + ComputeSeparation();
        Vector3 vel = rb.linearVelocity;
        vel.y = 0f;
        Vector3 dv = targetVel - vel;

        float accel = (targetVel.sqrMagnitude > vel.sqrMagnitude) ? acceleration : deceleration;
        float maxDv = accel * Time.fixedDeltaTime;
        if (dv.magnitude > maxDv) dv = dv.normalized * maxDv;

        Vector3 newVel = vel + dv;
        newVel.y = 0f;
        rb.linearVelocity = newVel;

        if (sprite && Mathf.Abs(rb.linearVelocity.x) > 0.01f)
            sprite.flipX = rb.linearVelocity.x < 0f;
    }

    Vector3 ComputeSeparation()
    {
        if (separationRadius <= 0f || enemyMask.value == 0) return Vector3.zero;
        Collider[] hits = Physics.OverlapSphere(transform.position, separationRadius, enemyMask);
        Vector3 force = Vector3.zero;
        foreach (Collider c in hits)
        {
            Rigidbody other = c.attachedRigidbody;
            if (!other || other == rb) continue;
            Vector3 away = transform.position - other.position;
            away.y = 0f;
            if (away.sqrMagnitude > 0.0001f)
                force += away.normalized / away.magnitude;
        }
        return force * separationForce;
    }

    void FaceTowards(Vector3 toPlayer)
    {
        if (toPlayer.sqrMagnitude < 0.0001f) return;
        Quaternion look = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
        Vector3 e = look.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, e.y, 0f);
    }

    void ChangeState(State next)
    {
        switch (state)
        {
            case State.Telegraph: ResetVisuals(); break;
        }

        state = next;

        switch (state)
        {
            case State.Strafe:
                if (Random.value < 0.25f) strafeDir *= -1;
                break;
            case State.Retreat:
                stateTimer = 0.4f + Random.Range(0f, 0.2f);
                break;
            case State.Telegraph:
                stateTimer = telegraphTime;
                if (sprite) sprite.color = telegraphColor;
                if (animator) animator.SetTrigger("Charge");
                break;
            case State.Charge:
                stateTimer = chargeDuration;
                break;
            case State.Stunned:
                if (animator) animator.SetBool("Stunned", true);
                break;
        }
    }

    void ResetVisuals()
    {
        if (sprite) sprite.color = normalColor;
        if (animator) animator.SetBool("Stunned", false);
    }

    void UpdateAnimator()
    {
        if (!animator) return;
        Vector3 flatVel = rb.linearVelocity; flatVel.y = 0f;
        animator.SetFloat("Speed", flatVel.magnitude);
    }

    /// <summary>Knockback & Stun (wird extern aufgerufen).</summary>
    public void Hit(Vector3 knockback, float stunTime = 0.2f)
    {
        rb.linearVelocity = Vector3.zero;
        knockback.y = 0f;
        rb.AddForce(knockback, ForceMode.Impulse);
        stunnedTimer = Mathf.Max(stunnedTimer, stunTime);
        ChangeState(State.Stunned);
    }
    
    void LateUpdate()
    {
        // Verhindert, dass sich der Gegner (das Sprite) dreht
        transform.rotation = Quaternion.identity;
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, strafeRange);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, chargeTriggerRange);
        Gizmos.color = Color.magenta; Gizmos.DrawWireSphere(transform.position, retreatRange);
        Gizmos.color = Color.green; Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}
