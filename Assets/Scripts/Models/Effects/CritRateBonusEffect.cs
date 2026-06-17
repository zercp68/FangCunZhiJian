using UnityEngine;

/// <summary>
/// 暴击率加成（金）
/// </summary>
public class CritRateBonusEffect : Effect
{
    public float critRateBonus;

    public override void Setup(CombatStats combatStats)
    {
        combatStats.critRateBonus += critRateBonus;
    }

    public override GameAction GetGameAction() => null;
}