using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombatController : MonoBehaviour
{
    private PlayerControls playerControls;
    private InputAction attack;
    private Animator animator;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.5f;
    private float lastAttackTime = -Mathf.Infinity;

    private void Awake()
    {
        playerControls = new PlayerControls();
        animator = GetComponent<Animator>();
        attack = playerControls.Combats.NormalAttack;
        attack.performed += ctx => PerformAttack();
    }

    private void OnEnable()
    {
        playerControls.Enable();
        attack.performed += ctx => PerformAttack();
        
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
}
