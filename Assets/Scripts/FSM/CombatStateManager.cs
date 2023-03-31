using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CombatStateManager : MonoBehaviour
{
    //Public State variables
    public CombatBaseState currentState;
    public CombatMeleeState meleeState = new CombatMeleeState();
    public CombatGrenadeState grenadeState = new CombatGrenadeState();
    public CombatShooterState shooterState = new CombatShooterState();

    //References
    PlayerInputs playerInput;

    [Header("Melee State")]
    [Space(10)]
    //Melee State variables
    public GameObject meleeWeapon;

    //Public Animator Override Controllers
    public List<AttackSO> Attacks;
    public List<AttackSO> JumpAttacks;

    [Header("Attack Animation Settings")]
    //Drop down menu for attack animations

    public float attackAnimSpeed = 1.5f;
    public float jumpAttackAnimSpeed = 2f;

    [Header("Damage Settings")]
    //Public Damage variables
    public float damageRadius = 1f;
    public LayerMask enemyLayers;
    public float weaponLength;
    public Transform weaponPoint;
    public Transform damagePoint;

    [Header("Jump Damage Settings")]
    //Public Jump Damage variables
    public float jumpDamageRadius = 1.5f;
    public LayerMask groundLayers;
    public float multliplierThreshold1 = 2f;
    public float multliplierThreshold2 = 2.5f;


    [Header("Grenade State")]
    [Space(10)]
    //Melee State variables

    public float throwForce = 40f;
    public GameObject grenadePrefab;

    [Header("Cinemachine")]
    public CinemachineVirtualCamera aimVirtualCamera;

    [Header("Display Controls")]
    public LineRenderer LineRenderer;
    public int linePoints = 25;
    public float timeBetweenPoints = 0.1f;
    public LayerMask GrenadeCollisionMask;

    // Start is called before the first frame update
    void Start()
    {
        //Get references
        playerInput = GetComponent<PlayerInputs>();

        //Get damage point
        damagePoint = transform.Find("DamagePoint");

        //Set initial state
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
            playerInput.grenadeState = false;
            playerInput.shooterState = false;
        }
        else if (playerInput.grenadeState && currentState != grenadeState)
        {
            SwitchState(grenadeState);
            playerInput.grenadeState = false;
            playerInput.meleeState = false;
            playerInput.shooterState = false;
        }
        else if (playerInput.shooterState && currentState != shooterState)
        {
            SwitchState(shooterState);
            playerInput.shooterState = false;
            playerInput.meleeState = false;
            playerInput.grenadeState = false;
        }
        currentState.UpdateState(this);
    }

    public void SwitchState(CombatBaseState newState)
    {
        currentState.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
    }

    void OnDrawGizmos()
    {
        if (damagePoint == null) damagePoint = transform.Find("DamagePoint");
        Gizmos.DrawWireSphere(damagePoint.position, damageRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(damagePoint.position, jumpDamageRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(weaponPoint.position, weaponPoint.position + weaponLength * -weaponPoint.transform.up);
    }
}
