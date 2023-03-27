using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "Attack")]

public class AttackSO : ScriptableObject
{
    public AnimatorOverrideController animatorOverride;
    public float damage = 10f;
    public float attackSpeed = 0.5f;
}
