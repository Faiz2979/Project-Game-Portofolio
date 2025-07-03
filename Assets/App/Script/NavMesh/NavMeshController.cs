using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
public class NavMeshController : MonoBehaviour
{
    public Camera mainCamera;
    private PlayerControls playerInput;
    private Vector2 mousePos;
    public NavMeshAgent agent;
    private Animator animator;

    void Awake()
    {

        animator = GetComponent<Animator>();
        playerInput = new PlayerControls();
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found. Please assign a camera to the PlayerController.");
        }
    }

    void Update()
    {
        AnimationController();
    }

    void OnEnable()
    {
        playerInput.Enable();
        playerInput.NavMesh.Move.performed += OnMove;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        mousePos = Mouse.current.position.ReadValue();
        // Debug.Log("Move action performed: " + mousePos);
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            // Debug.Log("Hit object: " + hit.collider.name);
            Debug.DrawLine(ray.origin, hit.point, Color.green, 60f);
            agent.SetDestination(hit.point);
        }
        else
        {
            Debug.Log("No hit detected.");
        }
    }

    bool IsGrounded()
    {
        // Check if the agent is grounded
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    void AnimationController()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);
        animator.SetBool("isGrounded", IsGrounded());
    }
}
