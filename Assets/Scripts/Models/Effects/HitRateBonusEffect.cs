using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitRateBonusEffect : Effect
{
    /// <summary>
    /// 攻击命中率加成(土牌)
    /// </summary>
    public float HitRateBonus;
    public override GameAction GetGameAction()
    {
        throw new System.NotImplementedException();
    }

    public override void Setup(CombatStats combatStats)
    {
        combatStats.hitRateBonus = HitRateBonus;
    }
}
