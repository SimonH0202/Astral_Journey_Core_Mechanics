using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CombatBaseState
{
    public abstract void EnterState(CombatStateManager manager);
    public abstract void UpdateState(CombatStateManager manager);
}

