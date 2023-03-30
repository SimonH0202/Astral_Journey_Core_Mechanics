using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatGrenadeState : CombatBaseState
{
    public override void EnterState(CombatStateManager manager)
    {
        //Log state change
        Debug.Log("Entered Grenade State");
    }

    public override void UpdateState(CombatStateManager manager)
    {
    }
}
