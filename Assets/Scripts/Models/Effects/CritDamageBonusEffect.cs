using UnityEngine;

/// <summary>
/// 暴击伤害加成效果（金·四金）
/// </summary>
public class CritDamageBonusEffect : Effect
{
    public float critDamageBonus;  // 0.3 = +30% 暴伤

    public override void Setup(CombatStats combatStats)
    {
        combatStats.critDamageBonus += critDamageBonus;
    }

    public override GameAction GetGameAction() => null;
}