using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerStatsManager stats;
    [SerializeField] private float fallbackSpeed = 5f;

    [Header("Rotation (Body only)")]
    [SerializeField] private Transform bodyPivot;      // NUR Model/Body hier rein (nicht AttackPivot)
    [SerializeField] private float turnSmoothTime = 0.1f;
    [SerializeField] private bool faceOppositeMove = true; // falls du weiterhin 180° willst

    private Rigidbody rb;
    private Vector2 moveInput;
    private float turnSmoothVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        float moveSpeed = stats ? stats.GetValue(CoreStatId.MoveSpeed) : fallbackSpeed;

        Vector3 dir = new Vector3(moveInput.x, 0f, moveInput.y);

        if (dir.sqrMagnitude > 0.001f)
        {
            dir.Normalize();

            Vector3 velocity = dir * moveSpeed;
            velocity.y = rb.linearVelocity.y;
            rb.linearVelocity = velocity;

            // Drehung nur am BodyPivot (AttackPivot bleibt unberührt)
            if (bodyPivot)
            {
                float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                if (faceOppositeMove) targetAngle += 180f;

                float smoothAngle = Mathf.SmoothDampAngle(
                    bodyPivot.eulerAngles.y,
                    targetAngle,
                    ref turnSmoothVelocity,
                    turnSmoothTime
                );

                bodyPivot.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
            }
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
