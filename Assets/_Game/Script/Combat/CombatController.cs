using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombatController : MonoBehaviour
{
    private PlayerControls playerControls;
    private InputAction attack;
    private InputAction move;
    private Animator animator;
    private Rigidbody rb;
    private Vector3 forceDirection = Vector3.zero;
    [SerializeField] private Camera playerCamera;
    Vector3 movementDirection;


    [Tooltip("Set the player movement speed")]
    [SerializeField] float moveSpeed = 5f;
    [Tooltip("Set the player max speed")]
    [SerializeField] float maxSpeed = 5f;
    [Tooltip("Capture the movement speed the player is moving")]
    [SerializeField] float speed;


    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.5f;
    private float lastAttackTime = -Mathf.Infinity;

    private void Awake()
    {
        playerControls = new PlayerControls();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        attack = playerControls.Combats.NormalAttack;
        attack.performed += ctx => PerformAttack();

    }

    private void OnEnable()
    {
        playerControls.Enable();
        attack.performed += ctx => PerformAttack();
        move = playerControls.Combats.Move;
        playerControls.Combats.ToggleMouseLock.performed += ToggleMouseLock;
    }



    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void PerformAttack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            animator.SetTrigger("Attack");
            // Add logic for dealing damage, hit detection, etc.
        }
    }


    void FixedUpdate()
    {
        Vector2 input = move.ReadValue<Vector2>();
        forceDirection = input.x * GetCameraRight(playerCamera) * moveSpeed;
        forceDirection += input.y * GetCameraForward(playerCamera) * moveSpeed;

        rb.AddForce(forceDirection, ForceMode.VelocityChange);
        forceDirection = Vector3.zero;

        Vector3 horizontalVelocity = rb.velocity;
        horizontalVelocity.y = 0f;

        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            Vector3 clamped = horizontalVelocity.normalized * maxSpeed;
            rb.velocity = new Vector3(clamped.x, rb.velocity.y, clamped.z);
        }

        // Ensure the player stops moving when no input is given
        if (rb.velocity.magnitude < 0.01f)
            rb.velocity = Vector3.zero;

        Debug.Log(rb.velocity.magnitude);
        LookAt();

        // Prevent rotation from physics
        rb.angularVelocity = Vector3.zero;
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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + movementDirection);
    }
}
