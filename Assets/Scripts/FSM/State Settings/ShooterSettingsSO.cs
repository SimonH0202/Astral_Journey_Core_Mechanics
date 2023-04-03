using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shooter Settings", menuName = "Settings/Shooter Settings")]

public class ShooterSettingsSO : ScriptableObject
{
    [Header("Prefab")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Shooter Settings")]
    [SerializeField] private float projectileSpeed = 100f;
    [SerializeField] private float shootingCooldown = 0.5f;
    [SerializeField] private float maxDistanceShooter = 50f;

    [Header("Charge Up Settings")]
    [SerializeField] private float startDamage = 5f;
    [SerializeField] private int targetDamage = 50;
    [SerializeField] private float chargeUpTime = 5f;

    [Header("Hip Fire Settings")]
    [SerializeField] private float hipFireCooldown = 0.1f;
    [SerializeField] private float hipFireDamage = 2f;

    //Getters
    public GameObject ProjectilePrefab { get { return projectilePrefab; } }
    public float ProjectileSpeed { get { return projectileSpeed; } }
    public float ShootingCooldown { get { return shootingCooldown; } }
    public float MaxDistanceShooter { get { return maxDistanceShooter; } }
    public float StartDamage { get { return startDamage; } }
    public int TargetDamage { get { return targetDamage; } }
    public float ChargeUpTime { get { return chargeUpTime; } }
    public float HipFireCooldown { get { return hipFireCooldown; } }
    public float HipFireDamage { get { return hipFireDamage; } }
}
