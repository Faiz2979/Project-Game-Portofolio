using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerControls playerControls;
    private InputAction move;

    private Rigidbody rb;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float maxSpeed = 5f;

    private Vector3 forceDirection = Vector3.zero;

    [Tooltip("Camera used to determine movement direction based on camera orientation.")]
    [SerializeField] private Camera playerCamera;

    [Header("Ground Check Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private Transform groundCheckPoint;

    [Header("Jump Timing Settings")]
    [Tooltip("Time allowed to jump after leaving the ground (coyote time).")]
    [SerializeField] private float coyoteTime = 0.1f;

    [Tooltip("Minimum time between jumps to prevent spamming.")]
    [SerializeField] private float jumpCooldown = 0.3f;

    private float lastGroundedTime;
    private float lastJumpTime = -Mathf.Infinity;

    private void Awake()
    {
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        playerControls.Player.Enable();
        move = playerControls.Player.Move;
        playerControls.Player.Jump.performed += OnJump;
    }

    private void OnDisable()
    {
        playerControls.Player.Disable();
        playerControls.Player.Jump.performed -= OnJump;
    }

    private void Update()
    {
        if (IsGrounded())
            lastGroundedTime = Time.time;
    }

    private void FixedUpdate()
    {
        Vector2 input = move.ReadValue<Vector2>();
        forceDirection = input.x * GetCameraRight(playerCamera) * moveSpeed;
        forceDirection += input.y * GetCameraForward(playerCamera) * moveSpeed;

        rb.AddForce(forceDirection, ForceMode.VelocityChange);

        Vector3 horizontalVelocity = rb.velocity;
        forceDirection = Vector3.zero;

        // Optional better gravity
        if (rb.velocity.y < 0f)
            rb.velocity += Vector3.up * Physics.gravity.y * Time.fixedDeltaTime;

        // Clamp max horizontal speed
        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            Vector3 clamped = horizontalVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(clamped.x, rb.velocity.y, clamped.z);
        }
        
        // Stop small movements
        if (rb.velocity.magnitude < 0.01f)
        {
            rb.velocity = Vector3.zero;
        }
        LookAt();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        bool canJump =
            (Time.time - lastGroundedTime <= coyoteTime) &&
            (Time.time - lastJumpTime >= jumpCooldown);

        if (canJump)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            lastJumpTime = Time.time;
        }
        else
        {
            Debug.Log("Jump blocked: not grounded or on cooldown");
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(groundCheckPoint.position, Vector3.down, groundCheckRadius, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundCheckPoint.position, groundCheckPoint.position + Vector3.down * groundCheckRadius);
        }
    }

    private Vector3 GetCameraRight(Camera playerCamera)
    {
        if (playerCamera == null)
        {
            throw new ArgumentNullException(nameof(playerCamera), "Player camera is not assigned.");
        }

        // Get the right direction of the camera
        Vector3 right = playerCamera.transform.right;
        right.y = 0; // Ignore vertical component
        return right.normalized; // Return the component for movement
    }

    private Vector3 GetCameraForward(Camera playerCamera)
    {
        if (playerCamera == null)
        {
            throw new ArgumentNullException(nameof(playerCamera), "Player camera is not assigned.");
        }

        // Get the forward direction of the camera
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0; // Ignore vertical component
        return forward.normalized; // Return the component for movement
    }


    private void LookAt()
    {
        Vector3 direction = rb.velocity;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.01f && move.ReadValue<Vector2>().sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * 10f);
        }
        else
        {
            rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.identity, Time.deltaTime * 10f);
            rb.angularVelocity = Vector3.zero;
        }
    }
}
