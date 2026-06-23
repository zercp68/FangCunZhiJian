using UnityEngine;

/// <summary>
/// 清空敌方增益效果
/// </summary>
public class BuffClearEffect : Effect
{
    public bool clearAllBuffs = true;

    public override void Setup(CombatStats combatStats) { }
    public override GameAction GetGameAction() => null;
}