using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 回血效果（基于造成伤害的百分比）
public class HealPercentEffect : Effect
{
    /// <summary>
    /// 基于目前的生命值回复的生命值百分比。
    /// 例如 0.2 表示回复目前生命值的 20%。
    /// </summary>
    public float HealPercent;      

    public override GameAction GetGameAction()
    {
        return null;
    }

    public override void Setup(CombatStats combatStats)
    {
        combatStats.healPercent += HealPercent;
    }
}
