using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Grenade Settings", menuName = "Settings/Grenade Settings")]

public class GrenadeSettingsSO : ScriptableObject
{
    [Header("Prefab")]
    [SerializeField] private GameObject grenadePrefab;

    [Header("Grenade Settings")]
    [SerializeField] private float throwForce = 40f;
    [SerializeField] private float grenadeCooldown = 1f;
    [SerializeField] private float maxDistance = 20f;

    [Header("Visual Settings")]
    [SerializeField] private int linePoints = 25;
    [SerializeField] private float timeBetweenPoints = 0.1f;

    [Header("Explosion Settings")]
    [SerializeField] private LayerMask GrenadeCollisionMask;

    //Getters
    public GameObject GrenadePrefab { get { return grenadePrefab; } }
    public float ThrowForce { get { return throwForce; } }
    public float GrenadeCooldown { get { return grenadeCooldown; } }
    public float MaxDistance { get { return maxDistance; } }
    public int LinePoints { get { return linePoints; } }
    public float TimeBetweenPoints { get { return timeBetweenPoints; } }
    public LayerMask GrenadeCollisionMask1 { get { return GrenadeCollisionMask; } }
}
