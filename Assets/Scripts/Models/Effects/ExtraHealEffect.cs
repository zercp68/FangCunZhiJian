using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 固定回血（比如四木额外回复失去生命的50%）
public class ExtraHealEffect : Effect
{
    /// <summary>
    //基于已损失生命值的回复比例。例如 0.5 表示回复已损失生命的 50%。
    /// </summary>
    public float ExtraHealFromLost; 

    public override GameAction GetGameAction()
    {
        return null;
    }

    public override void Setup(CombatStats combatStats)
    {
        combatStats.extraHealFromLost += ExtraHealFromLost;
    }
}
