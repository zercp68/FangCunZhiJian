using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 效果数值传递结构体（用于属性牌 Effect 的数据汇总）
/// </summary>
public class CombatStats
{
    [Header("金系")]
    public float critRateBonus;      // 暴击率加成（如 0.2 表示 +20%）

    [Header("木系")]
    public float healPercent;        // 基于本次攻击伤害的回血比例（如 0.2 表示回复伤害的20%）
    public int extraHealFromLost;        // 固定额外回血值（四木 +5，四水 +3）

    [Header("水系")]
    public float allStatBonus;       // 全属性加成（攻击、暴击等百分比提升）

    [Header("火系")]
    public float damagePercentBonus; // 伤害百分比加成（如 0.2 表示 +20%）
    public int burnStacks;           // 灼烧层数（每层造成2点真实伤害）

    [Header("土系")]
    public int defenseFlatBonus;     // 固定防御加成（如 +2 防御）
}