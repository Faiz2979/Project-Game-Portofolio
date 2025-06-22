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

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float dashTimer = 0f;
    private bool isDashing = false;
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

        else
        {
            float initialGravity = Physics.gravity.y;
            Physics.gravity = new Vector3(0, -9.81f, 0);
            rb.AddForce(Vector3.up * initialGravity, ForceMode.Acceleration);
        }

        dashTimer -= Time.deltaTime;
        AnimatorController();
    }

    private void FixedUpdate(){
        Vector2 input = move.ReadValue<Vector2>();
        forceDirection = input.x * GetCameraRight(playerCamera) * moveSpeed;
        forceDirection += input.y * GetCameraForward(playerCamera) * moveSpeed;

        rb.AddForce(forceDirection, ForceMode.VelocityChange);
        forceDirection = Vector3.zero;

        Vector3 horizontalVelocity = rb.velocity;
        horizontalVelocity.y = 0f;

        // Optional better gravity
        if (rb.velocity.y < 0f)
            rb.velocity += Vector3.up * Physics.gravity.y * Time.fixedDeltaTime;

        // Clamp speed only if not dashing
        if (!isDashing && horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            Vector3 clamped = horizontalVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(clamped.x, rb.velocity.y, clamped.z);
        }

        if (rb.velocity.magnitude < 0.01f)
            rb.velocity = Vector3.zero;

        LookAt();

        // Prevent rotation from physics
        rb.angularVelocity = Vector3.zero;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        bool canJump = (Time.time - lastGroundedTime <= coyoteTime) &&
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

    private void OnDash(InputAction.CallbackContext context)
    {
        if (isDashing || dashTimer > 0) return;
        dashTimer = dashCooldown;
        Vector2 input = move.ReadValue<Vector2>();
        if (input.sqrMagnitude < 0.1f)
        {
            Debug.Log("Dash blocked: no movement input");
            return;
        }

        Vector3 dashDirection = input.x * GetCameraRight(playerCamera) + input.y * GetCameraForward(playerCamera);
        dashDirection.Normalize();

        StartCoroutine(DashCoroutine(dashDirection));
    }

    private IEnumerator DashCoroutine(Vector3 direction)
    {
        isDashing = true;
        rb.velocity = direction * dashSpeed;
        Debug.Log("Dash performed!");
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }

    public bool IsGrounded()
    {
        Vector3 origin = groundCheckPoint.position;
        Vector3 left = origin + groundCheckPoint.right * groundCheckRadius * 0.5f;
        Vector3 right = origin - groundCheckPoint.right * groundCheckRadius * 0.5f;

        bool centerHit = Physics.Raycast(origin, Vector3.down, groundCheckRadius, groundLayer);
        bool leftHit = Physics.Raycast(left, Vector3.down, groundCheckRadius, groundLayer);
        bool rightHit = Physics.Raycast(right, Vector3.down, groundCheckRadius, groundLayer);

        return centerHit || leftHit || rightHit;
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

    private Vector3 GetCameraRight(Camera playerCamera)
    {
        Vector3 right = playerCamera.transform.right;
        right.y = 0;
        return right.normalized;
    }

    private Vector3 GetCameraForward(Camera playerCamera)
    {
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    private void LookAt()
    {
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

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Vector3 origin = groundCheckPoint.position;
            Vector3 left = origin + groundCheckPoint.right * groundCheckRadius * 0.5f;
            Vector3 right = origin - groundCheckPoint.right * groundCheckRadius * 0.5f;

            Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckRadius);
            Gizmos.DrawLine(left, left + Vector3.down * groundCheckRadius);
            Gizmos.DrawLine(right, right + Vector3.down * groundCheckRadius);
        }
    }
}
