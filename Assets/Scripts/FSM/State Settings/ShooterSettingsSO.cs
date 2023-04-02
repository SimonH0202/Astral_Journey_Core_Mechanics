using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shooter Settings", menuName = "Settings/Shooter Settings")]

public class ShooterSettingsSO : ScriptableObject
{
    [Header("Prefab")]
    public GameObject projectilePrefab;

    [Header("Shooter Settings")]
    public float projectileSpeed = 100f;
    public float shootingCooldown = 0.5f;
    public float maxDistanceShooter = 50f;
}
