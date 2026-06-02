using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 效果数值传递结构体
/// </summary>
public struct CombatStats
{
    /// <summary>
    /// 暴击率加成（金）
    /// </summary>
    public float critRateBonus;
    /// <summary>
    /// 回血比例（目前血量的比例）（木）
    /// </summary>
    public float healPercent;
    /// <summary>
    /// 全属性加成（水）
    /// </summary>
    public float allStatBonus;
    /// <summary>
    /// 伤害百分比加成（火）
    /// </summary>
    public float damagePercentBonus;
    /// <summary>
    /// 灼烧层数（火）
    /// </summary>
    public int burnStacks;
    /// <summary>
    /// 固定防御加成层（土）
    /// </summary>
    public int defenseFlatBonus;
    /// <summary>
    /// 额外回复失去生命比例（四木/四水）
    /// </summary>
    public float extraHealFromLost;
    /// <summary>
    /// 攻击命中率加成(土牌)
    /// </summary>
    public float hitRateBonus;

}
