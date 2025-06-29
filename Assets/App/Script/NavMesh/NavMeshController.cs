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

    void Awake()
    {
        playerInput = new PlayerControls();
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found. Please assign a camera to the PlayerController.");
        }
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
}
