using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//暴击率加成（金）
public class CritRateBonusEffect : Effect
{
    /// <summary>
    /// 暴击率加成（金）
    /// </summary>
    public float CritRateBonus;
    public override GameAction GetGameAction()
    {
        return null;
    }

    public override void Setup(CombatStats combatStats)
    {
        combatStats.critRateBonus += CritRateBonus;
    }

}
