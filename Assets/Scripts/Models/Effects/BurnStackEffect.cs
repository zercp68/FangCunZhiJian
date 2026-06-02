using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ×ÆÉÕÐ§¹û
public class BurnStackEffect : Effect
{
    /// <summary>
    /// ×ÆÉÕ²ãÊý
    /// </summary>
    public int BurnStacks;         

    public override GameAction GetGameAction()
    {
        return null;
    }

    public override void Setup(CombatStats combatStats)
    {
        combatStats.burnStacks += BurnStacks;
    }
}
