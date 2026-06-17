using UnityEngine;

/// <summary>
/// 水全属性加成
/// </summary>
public class AllStatBonusEffect : Effect
{
    public float allStatBonus;

    public override void Setup(CombatStats combatStats)
    {
        combatStats.allStatBonus += allStatBonus;
    }

    public override GameAction GetGameAction() => null;
}