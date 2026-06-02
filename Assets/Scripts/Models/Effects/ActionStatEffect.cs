using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 辅助 Effect 类型（需要放在你的 Effect.cs 同文件或单独文件）
public enum ActionStatType
{
    /// <summary>
    /// 暴击率
    /// </summary>
    CritRateBonus,
    /// <summary>
    /// 暴击伤害
    /// </summary>
    CritDamageBonus,
    /// <summary>
    /// 伤害倍率
    /// </summary>
    DamageMultiplier,
    /// <summary>
    /// 防御加成
    /// </summary>
    DefenseBonus
}

[System.Serializable]
public class ActionStatEffect : Effect
{
    public ActionStatType statType;
    public float value;

    public override GameAction GetGameAction()
    {
        // 数值加成型 Effect 不直接产生 GameAction，返回 null
        return null;
    }

    public override void Setup(CombatStats combatStats)
    {
        
    }
}