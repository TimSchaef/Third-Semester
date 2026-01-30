using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerStatsManager stats;
    [SerializeField] private float fallbackSpeed = 5f;

    [Header("Rotation / Visuals")]
    [SerializeField] private Transform bodyPivot;
    [SerializeField] private SpriteRenderer sprite;   
    [SerializeField] private Animator animator;       

    private Rigidbody rb;
    private Vector2 moveInput;

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
            Vector3 v = dir * moveSpeed;
            v.y = rb.linearVelocity.y;
            rb.linearVelocity = v;
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    private void LateUpdate()
    {
        if (bodyPivot && Mathf.Abs(moveInput.x) > 0.01f)
        {
            Vector3 scale = bodyPivot.localScale;
            scale.x = moveInput.x < 0f ? -1f : 1f;
            bodyPivot.localScale = scale;
        }

        if (animator)
            animator.SetFloat("Speed", rb.linearVelocity.magnitude);
    }




    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}

