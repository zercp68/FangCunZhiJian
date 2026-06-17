using UnityEngine;
/// <summary>
/// 固定防御加成（土）
/// </summary>
public class DefenseFlatBonusEffect : Effect
{
    public int defenseFlatBonus;

    public override void Setup(CombatStats combatStats)
    {
        combatStats.defenseFlatBonus += defenseFlatBonus;
    }

    public override GameAction GetGameAction() => null;
}