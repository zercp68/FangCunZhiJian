using UnityEngine;
/// <summary>
/// 伤害百分比加成（火）
/// </summary>
public class DamagePercentBonusEffect : Effect
{
    public float damagePercentBonus;

    public override void Setup(CombatStats combatStats)
    {
        combatStats.damagePercentBonus += damagePercentBonus;
    }

    public override GameAction GetGameAction() => null;
}