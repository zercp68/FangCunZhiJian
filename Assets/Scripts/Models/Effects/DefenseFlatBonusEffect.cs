using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 固定防御加成（土）
public class DefenseFlatBonusEffect : Effect
{
    /// <summary>
    /// 固定防御加成（土）
    /// </summary>
    public int DefenseFlatBonus;
    public override GameAction GetGameAction()
    {
        throw new System.NotImplementedException();
    }

    public override void Setup(CombatStats combatStats)
    {
        combatStats.defenseFlatBonus += DefenseFlatBonus;
    }
}
