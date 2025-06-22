using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerControls playerControls;
    private InputAction move;
    private Animator animator;

    private Rigidbody rb;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] public float maxSpeed = 5f;

    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.2f;

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
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        playerControls.Player.Enable();
        move = playerControls.Player.Move;
        playerControls.Player.Jump.performed += OnJump;
        playerControls.Player.ToggleMouseLock.performed += ToggleMouseLock;
        playerControls.Player.Dash.performed += OnDash;

    }


    private void OnDisable()
    {
        playerControls.Player.Disable();
        playerControls.Player.Jump.performed -= OnJump;
        playerControls.Player.Dash.performed -= OnDash;
        playerControls.Player.ToggleMouseLock.performed -= ToggleMouseLock;
    }

    private void Update()
    {
        if (IsGrounded())
            lastGroundedTime = Time.time;

        AnimatorController();
    }

    private void ToggleMouseLock(InputAction.CallbackContext context)
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

private void OnDash(InputAction.CallbackContext context){

    Vector2 input = move.ReadValue<Vector2>();

    // Cegah dash jika tidak ada input arah
    if (input.sqrMagnitude < 0.1f)
    {
        Debug.Log("Dash blocked: no movement input");
        return;
    }

    // Hitung arah dash dari input dan kamera
    Vector3 dashDirection = input.x * GetCameraRight(playerCamera) + input.y * GetCameraForward(playerCamera);
    Debug.Log("Dash direction: " + dashDirection);
    dashDirection.Normalize();

    // Terapkan gaya dash
    rb.velocity = dashDirection * dashSpeed;
    Debug.Log("Dash performed!");
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

        rb.angularVelocity = Vector3.zero; // Prevent rotation from physics
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

    public bool IsGrounded()
    {
        // Perform 3 raycasts: center, left, and right
        Vector3 origin = groundCheckPoint.position;
        Vector3 left = origin + groundCheckPoint.right * groundCheckRadius * 0.5f;
        Vector3 right = origin - groundCheckPoint.right * groundCheckRadius * 0.5f;

        bool centerHit = Physics.Raycast(origin, Vector3.down, groundCheckRadius, groundLayer);
        bool leftHit = Physics.Raycast(left, Vector3.down, groundCheckRadius, groundLayer);
        bool rightHit = Physics.Raycast(right, Vector3.down, groundCheckRadius, groundLayer);

        return centerHit || leftHit || rightHit;
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

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Vector3 origin = groundCheckPoint.position;
            Vector3 left = origin + groundCheckPoint.right * groundCheckRadius * 0.5f;
            Vector3 right = origin - groundCheckPoint.right * groundCheckRadius * 0.5f;
            Gizmos.DrawLine(groundCheckPoint.position, groundCheckPoint.position + Vector3.down * groundCheckRadius);
            Gizmos.DrawLine(left, left + Vector3.down * groundCheckRadius);
            Gizmos.DrawLine(right, right + Vector3.down * groundCheckRadius);
        }
    }

    private void LookAt(){
        Vector2 input = move.ReadValue<Vector2>();
        if (input.sqrMagnitude > 0.01f)
        {
            Vector3 direction = input.x * GetCameraRight(playerCamera) + input.y * GetCameraForward(playerCamera);
            direction.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * 10f));
        }
    }


    private void AnimatorController()
    {
        animator.SetFloat("Speed", rb.velocity.magnitude / maxSpeed);
        animator.SetBool("isGrounded", IsGrounded());
        animator.SetFloat("verticalVelocity", rb.velocity.y);

    }

}
