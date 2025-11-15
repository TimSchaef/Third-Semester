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
    private Vector2 moveInput;
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

        // Bewegungsrichtung berechnen
        Vector3 dir = new Vector3(moveInput.x, 0f, moveInput.y);

        if (dir.sqrMagnitude > 0.001f)
        {
            dir.Normalize();

            // Bewegung anwenden
            Vector3 velocity = dir * moveSpeed;
            velocity.y = rb.linearVelocity.y;
            rb.linearVelocity = velocity;

            // --- TurnPivot in entgegengesetzte Richtung der Bewegung drehen ---
            if (turnPivot)
            {
                // Zielwinkel bestimmen
                float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

                // Drehung um 180° (entgegengesetzt)
                targetAngle += 180f;

                // Weiche Rotation
                float smoothAngle = Mathf.SmoothDampAngle(
                    turnPivot.eulerAngles.y,
                    targetAngle,
                    ref turnSmoothVelocity,
                    turnSmoothTime
                );

                // Rotation anwenden
                turnPivot.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
            }
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        Debug.DrawRay(transform.position, dir * 2f, Color.green);
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Debug.Log($"Move Input: {moveInput}");
    }
}