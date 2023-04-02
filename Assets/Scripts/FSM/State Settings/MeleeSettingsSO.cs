using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Settings", menuName = "Settings/Melee Settings")]

public class MeleeSettingsSO : ScriptableObject
{
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

    [Header("Jump Damage Settings")]
    //Public Jump Damage variables
    public float jumpDamageRadius = 1.5f;
    public LayerMask groundLayers;
    public float multliplierThreshold1 = 2f;
    public float multliplierThreshold2 = 2.5f;
}
