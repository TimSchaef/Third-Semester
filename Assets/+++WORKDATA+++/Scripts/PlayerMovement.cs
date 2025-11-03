using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private Rigidbody rb;
    [SerializeField] private Transform turnPivot;

    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Move on XZ, keep current Y velocity (gravity/jumps)
        Vector3 v = new Vector3(moveInput.x, rb.linearVelocity.y, moveInput.y) * speed;
        v.y = rb.linearVelocity.y; // ensure Y not scaled by speed from input
        rb.linearVelocity = v;

        if (moveInput != Vector2.zero && turnPivot != null)
        {
            Vector3 dir = new Vector3(moveInput.x, 0f, moveInput.y);
            turnPivot.rotation = Quaternion.LookRotation(-dir, Vector3.up);
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}