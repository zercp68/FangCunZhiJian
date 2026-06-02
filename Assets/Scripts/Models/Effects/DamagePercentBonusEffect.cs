using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 伤害百分比加成（火）
public class DamagePercentBonusEffect : Effect
{
    /// <summary>
    /// 伤害百分比加成（火）
    /// </summary>
    public float DamagePercentBonus;
    public override GameAction GetGameAction()
    {
        throw new System.NotImplementedException();
    }

    public override void Setup(CombatStats combatStats)
    {
        combatStats.damagePercentBonus += DamagePercentBonus;
    }
}
