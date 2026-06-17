using UnityEngine;

/// <summary>
/// 动作卡数值加成
/// </summary>
public enum ActionStatType
{
    CritRateBonus,      // 暴击率加成
    CritDamageBonus,    // 暴击伤害加成
    DamageMultiplier,   // 伤害倍率
    DefenseBonus        // 防御加成
}

[System.Serializable]
public class ActionStatEffect : Effect
{
    public ActionStatType statType;
    public float value;

    public override void Setup(CombatStats combatStats)
    {
        // 动作卡不向 CombatStats 添加数值，留空
    }

    public override GameAction GetGameAction() => null;
}