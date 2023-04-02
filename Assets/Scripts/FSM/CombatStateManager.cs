using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Animations.Rigging;

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
    public MeleeSettingsSO meleeSettings;

    [Header("Grenade State")]
    [Space(10)]
    //Grenade State variables
    public GrenadeSettingsSO grenadeSettings;

    [Header("Shooter State")]
    [Space(10)]
    //Shooter State variables
    public GameObject crosshair;
    public ShooterSettingsSO shooterSettings;

    [Header("Cinemachine")]
    public CinemachineVirtualCamera aimVirtualCamera;
    public float aimSenitivity = 0.5f;

    [Header("General Animation Settings")]
    //General Animation variables
    public Transform armAimPoint;
    public Rig aimRig;
  

 
    
    // Start is called before the first frame update
    void Start()
    {
        //Get references
        playerInput = GetComponent<PlayerInputs>();

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
}
