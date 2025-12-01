using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerStatsManager stats;
    [SerializeField] private float fallbackSpeed = 5f;
    [SerializeField] private Transform turnPivot;
    [SerializeField] private float turnSmoothTime = 0.1f;

    private Rigidbody rb;

    // Bewegung (WASD / Stick)
    private Vector2 moveInput;
    // Zielen / Attack-Richtung (Pfeiltasten)
    private Vector2 aimInput;

    private float moveSpeed;
    private float turnSmoothVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        moveSpeed = stats ? stats.GetValue(CoreStatId.MoveSpeed) : fallbackSpeed;

        // -------------------------
        // 1) BEWEGUNG (WASD)
        // -------------------------
        Vector3 dir = new Vector3(moveInput.x, 0f, moveInput.y);

        if (dir.sqrMagnitude > 0.001f)
        {
            dir.Normalize();

            Vector3 velocity = dir * moveSpeed;
            velocity.y = rb.linearVelocity.y;
            rb.linearVelocity = velocity;
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        Debug.DrawRay(transform.position, dir * 2f, Color.green);

        // -------------------------
        // 2) TURNPIVOT (PFEILTASTEN)
        // -------------------------
        if (turnPivot)
        {
            // Pfeiltasten-Eingabe → aimInput (siehe Aim()-Methode)
            if (aimInput.sqrMagnitude > 0.001f)
            {
                Vector3 aimDir = new Vector3(aimInput.x, 0f, aimInput.y).normalized;

                float targetAngle = Mathf.Atan2(aimDir.x, aimDir.z) * Mathf.Rad2Deg;
                targetAngle += 180f; // falls du weiterhin "entgegengesetzt" willst

                float smoothAngle = Mathf.SmoothDampAngle(
                    turnPivot.eulerAngles.y,
                    targetAngle,
                    ref turnSmoothVelocity,
                    turnSmoothTime
                );

                turnPivot.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
            }
            // wenn keine Pfeiltaste gedrückt wird, bleibt turnPivot einfach wie er ist
        }
    }

    // Wird vom Move-Input (WASD / Stick) aufgerufen
    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        // Debug.Log($"Move Input: {moveInput}");
    }

    // NEU: Wird von einem separaten "Aim"-Action (Pfeiltasten) aufgerufen
    public void Aim(InputAction.CallbackContext context)
    {
        aimInput = context.ReadValue<Vector2>();
        // Debug.Log($"Aim Input: {aimInput}");
    }
}
