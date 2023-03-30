using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatStateManager : MonoBehaviour
{
    public CombatBaseState currentState;

    public CombatMeleeState meleeState = new CombatMeleeState();
    public CombatGrenadeState grenadeState = new CombatGrenadeState();
    public CombatShooterState shooterState = new CombatShooterState();

    //Melee State variables
    [Header("Melee State")]

    //References
    PlayerInputs playerInput;
    Animator animator;
    MovementController movementController;

    //Public Animator Override Controllers
    public List<AttackSO> Attacks;
    public List<AttackSO> JumpAttacks;

    [Header("Attack Animation Settings")]
    //Public Attack Animation variables
    public float attackAnimSpeed = 1.5f;
    public float jumpAttackAnimSpeed = 2f;

    [Header("Damage Settings")]
    //Public Damage variables
    public float damageRadius = 1f;
    public LayerMask enemyLayers;
    public float weaponLength;
    public Transform weaponPoint;

    [Header("Jump Damage Settings")]
    //Public Jump Damage variables
    public float jumpDamageRadius = 1.5f;
    public LayerMask groundLayers;
    public float multliplierThreshold1 = 2f;
    public float multliplierThreshold2 = 2.5f;

    // Start is called before the first frame update
    void Start()
    {
        //Get references
        playerInput = GetComponent<PlayerInputs>();
        animator = GetComponent<Animator>();
        movementController = GetComponent<MovementController>();

        currentState = meleeState;
        currentState.EnterState(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInput.meleeState && currentState != meleeState)
        {
            SwitchState(meleeState);
            playerInput.meleeState = false;
        }
        else if (playerInput.grenadeState && currentState != grenadeState)
        {
            SwitchState(grenadeState);
            playerInput.grenadeState = false;
        }
        else if (playerInput.shooterState && currentState != shooterState)
        {
            SwitchState(shooterState);
            playerInput.shooterState = false;
        }
        currentState.UpdateState(this);
    }

    public void SwitchState(CombatBaseState newState)
    {
        currentState = newState;
        currentState.EnterState(this);
    }

    //Getters

}
