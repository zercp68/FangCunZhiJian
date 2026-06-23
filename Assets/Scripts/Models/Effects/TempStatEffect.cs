using UnityEngine;

/// <summary>
/// 临时数值加成类型
/// </summary>
public enum TempStatType
{
    AttackPercent,      // 攻击百分比加成（火/土）
    DefenseFlat,        // 防御固定值加成（土）
    CritRate,           // 暴击率加成（金）
    CritDamage,         // 暴击伤害加成（金四金）
    HitRate,            // 命中率加成（暂未使用）
    AllStatPercent,     // 全属性百分比加成（水）
    DamagePercent,      // 伤害百分比加成（火）
    BurnStacks,         // 灼烧层数（火）
    HealPercent,        // 回血比例（木）
    ExtraHeal,          // 固定额外回血（）
}

/// <summary>
/// 通用临时数值加成效果
/// </summary>
[System.Serializable]
public class TempStatEffect : Effect
{
    public TempStatType statType;
    public float value;        // 百分比用 0~1，固定值直接用数值（如防御+2，填2）

    public override void Setup(CombatStats combatStats)
    {
        switch (statType)
        {
            case TempStatType.AttackPercent:
                combatStats.damagePercentBonus += value;
                break;
            case TempStatType.DefenseFlat:
                combatStats.defenseFlatBonus += (int)value;
                break;
            case TempStatType.CritRate:
                combatStats.critRateBonus += value;
                break;
            case TempStatType.CritDamage:
                combatStats.critDamageBonus += value;
                break;
            case TempStatType.AllStatPercent:
                combatStats.allStatBonus += value;
                break;
            case TempStatType.DamagePercent:
                combatStats.damagePercentBonus += value;
                break;
            case TempStatType.BurnStacks:
                combatStats.burnStacks += (int)value;
                break;
            case TempStatType.HealPercent:
                combatStats.healPercent += value;
                break;
            case TempStatType.ExtraHeal:
                combatStats.extraHealFromLost += (int)value;
                break;
            case TempStatType.HitRate:
                // 暂未使用，预留
                break;
        }
    }

    public override GameAction GetGameAction() => null;
}