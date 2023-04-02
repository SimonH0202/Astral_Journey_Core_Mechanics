using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Grenade Settings", menuName = "Settings/Grenade Settings")]

public class GrenadeSettingsSO : ScriptableObject
{
    [Header("Prefab")]
    public GameObject grenadePrefab;

    [Header("Grenade Settings")]
    public float throwForce = 40f;
    public float grenadeCooldown = 1f;
    public float maxDistance = 20f;

    [Header("Visual Settings")]
    public int linePoints = 25;
    public float timeBetweenPoints = 0.1f;

    [Header("Explosion Settings")]
    public LayerMask GrenadeCollisionMask;
}
