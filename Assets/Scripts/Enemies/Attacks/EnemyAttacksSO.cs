using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New EnemyAttack", menuName = "EnemyAttack")]

public class EnemyAttacksSO : ScriptableObject
{
    [Header("Animation Settings")]
    [SerializeField] private AnimatorOverrideController animatorOverride;
    [SerializeField] private float animationSpeed = 1f;
    [Header("Attack Settings")]
    [SerializeField] private float damage = 30f;
    [SerializeField] private float attackTime = 0.5f;
    [SerializeField] private float cooldown = 3f;
    [SerializeField] private float damageDelay = 0.5f;
    [SerializeField] private AttackTypes attackType;
    [SerializeField] private float AOERadius = 3f;

    //private variables
    private float attackTimer = 0f;
    private bool attackOnCooldown = false;

    public enum AttackTypes
    {
        Raycast,
        AOE
    }

    void OnEnable()
    {
        // Reset variables
        attackTimer = 0f;
        attackOnCooldown = false;
    }

    //Getters and Setters
    public AnimatorOverrideController AnimatorOverride { get { return animatorOverride; } }
    public float AnimationSpeed { get { return animationSpeed; } }
    public float Damage { get { return damage; } }
    public float AttackTime { get { return attackTime; } }
    public float Cooldown { get { return cooldown; } }
    public float AttackTimer { get { return attackTimer; } set { attackTimer = value; } }
    public float DamageDelay { get { return damageDelay; } }
    public bool AttackOnCooldown { get { return attackOnCooldown; } set { attackOnCooldown = value; } }
    public AttackTypes AttackType { get { return attackType; } }
    public float AoERadius { get { return AOERadius; } }

}
