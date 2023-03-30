using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatShooterState : CombatBaseState
{
    public override void EnterState(CombatStateManager manager)
    {
        //Log state change
        Debug.Log("Entered Shooter State");
    }

    public override void UpdateState(CombatStateManager manager)
    {
    }

    public override void ExitState(CombatStateManager manager)
    {
    }
}
