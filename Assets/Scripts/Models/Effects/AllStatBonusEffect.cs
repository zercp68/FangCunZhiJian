using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 全属性加成（水）
public class AllStatBonusEffect : Effect
{
    /// <summary>
    /// 全属性加成（水）
    /// </summary>
    public float AllStatBonus;
    public override GameAction GetGameAction()
    {
        throw new System.NotImplementedException();
    }

    public override void Setup(CombatStats combatStats)
    {
        combatStats.allStatBonus += AllStatBonus;
    }
}
