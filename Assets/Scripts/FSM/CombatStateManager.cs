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
    [SerializeField] private GameObject meleeWeapon;
    [SerializeField] private MeleeSettingsSO meleeSettings;

    [Header("Grenade State")]
    [Space(10)]
    //Grenade State variables
    [SerializeField] private GrenadeSettingsSO grenadeSettings;

    [Header("Shooter State")]
    [Space(10)]
    //Shooter State variables
    [SerializeField] private GameObject crosshair;
    [SerializeField] private ShooterSettingsSO shooterSettings;

    [Header("Cinemachine")]
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private float aimSenitivity = 0.5f;

    [Header("General Animation Settings")]
    //General Animation variables
    [SerializeField] private Transform armAimPoint;
    [SerializeField] private Rig aimRig;
    [SerializeField] private Rig hipFireRig;
 
    
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

    //Getters
    public GameObject MeleeWeapon { get { return meleeWeapon; } }
    public MeleeSettingsSO MeleeSettings { get { return meleeSettings; } }
    public GrenadeSettingsSO GrenadeSettings { get { return grenadeSettings; } }
    public ShooterSettingsSO ShooterSettings { get { return shooterSettings; } }
    public Transform ArmAimPoint { get { return armAimPoint; } }
    public Rig AimRig { get { return aimRig; } }
    public Rig HipFireRig { get { return hipFireRig; } }
    public CinemachineVirtualCamera AimVirtualCamera { get { return aimVirtualCamera; } }
    public float AimSenitivity { get { return aimSenitivity; } }
    public GameObject Crosshair { get { return crosshair; } }
}
