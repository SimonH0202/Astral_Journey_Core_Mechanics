using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Settings", menuName = "Settings/Melee Settings")]

public class MeleeSettingsSO : ScriptableObject
{
    //Public Animator Override Controllers
    [SerializeField] private List<AttackSO> Attacks;
    [SerializeField] private List<AttackSO> JumpAttacks;

    [Header("Attack Animation Settings")]
    //Public Attack Animation variables
    [SerializeField] private float attackAnimSpeed = 1.5f;
    [SerializeField] private float jumpAttackAnimSpeed = 2f;

    [Header("Damage Settings")]
    //Public Damage variables
    [SerializeField] private float damageRadius = 1f;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private float weaponLength;

    [Header("Jump Damage Settings")]
    //Public Jump Damage variables
    [SerializeField] private float jumpDamageRadius = 1.5f;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float multliplierThreshold1 = 2f;
    [SerializeField] private float multliplierThreshold2 = 2.5f;

    //Getters
    public List<AttackSO> AttacksList { get { return Attacks; } }
    public List<AttackSO> JumpAttacksList { get { return JumpAttacks; } }
    public float AttackAnimSpeed { get { return attackAnimSpeed; } }
    public float JumpAttackAnimSpeed { get { return jumpAttackAnimSpeed; } }
    public float DamageRadius { get { return damageRadius; } }
    public LayerMask EnemyLayers { get { return enemyLayers; } }
    public float WeaponLength { get { return weaponLength; } }
    public float JumpDamageRadius { get { return jumpDamageRadius; } }
    public LayerMask GroundLayers { get { return groundLayers; } }
    public float MultliplierThreshold1 { get { return multliplierThreshold1; } }
    public float MultliplierThreshold2 { get { return multliplierThreshold2; } }
    
}
