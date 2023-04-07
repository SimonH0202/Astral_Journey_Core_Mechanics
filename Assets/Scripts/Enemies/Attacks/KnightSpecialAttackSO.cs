using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New KnightSpecialAttack", menuName = "KnightSpecialAttack")]

public class KnightSpecialAttackSO : ScriptableObject
{
    // Inspector variables
    [Header("Animation Settings")]
    [SerializeField] private AnimatorOverrideController animatorOverride;
    [SerializeField] private float animationSpeed = 1f;
    [Header("Laser Settings")]
    [SerializeField] private float damage = 5f;
    [SerializeField] private float laserSpeed = 10f;
    [SerializeField] private float laserRange = 10f;
    [SerializeField] private float cooldown = 5f;
    [SerializeField] private float sameDirectionThreshold = 1f;

    // Private variables
    private float laserTimer = 0f;
    private float laserDistance = 0f;
    private float laserDistanceIncrease = 1f;
    private Vector3 laserTarget;
    private bool attackOnCooldown = true;
    private float attackTimer = 0f;

    // Private Laser accuracy variables
    private float sameDirectionTimer = 0f;

    void OnEnable()
    {
        // Reset variables
        laserTimer = 0f;
        laserDistance = 0f;
        laserDistanceIncrease = 1f;
        attackOnCooldown = true;
        attackTimer = 0f;
        sameDirectionTimer = 0f;
    }

    // Laser Getters and Setters
    public float LaserSpeed { get { return laserSpeed; } }
    public float LaserRange { get { return laserRange; } }
    public float LaserTimer { get { return laserTimer; } set { laserTimer = value; } }
    public float LaserDistance { get { return laserDistance; } set { laserDistance = value; } }
    public float LaserDistanceIncrease { get { return laserDistanceIncrease; } set { laserDistanceIncrease = value; } }
    public Vector3 LaserTarget { get { return laserTarget; } set { laserTarget = value; } }
    
    // Animation getters and setters
    public AnimatorOverrideController AnimatorOverride { get { return animatorOverride; } }
    public float AnimationSpeed { get { return animationSpeed; } }

    // Attack getters and setters
    public bool AttackOnCooldown { get { return attackOnCooldown; } set { attackOnCooldown = value; } }
    public float AttackTimer { get { return attackTimer; } set { attackTimer = value; } }
    public float Damage { get { return damage; } }
    public float Cooldown { get { return cooldown; } }

    // Laser accuracy getters and setters
    public float SameDirectionTimer { get { return sameDirectionTimer; } set { sameDirectionTimer = value; } }
    public float SameDirectionThreshold { get { return sameDirectionThreshold; } set { sameDirectionThreshold = value; } }
}
