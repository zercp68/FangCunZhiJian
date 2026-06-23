using UnityEngine;

/// <summary>
/// 免疫一次攻击效果
/// </summary>
public class ImmuneEffect : Effect
{
    public int immuneCount = 1;   // 默认免疫1次

    public override void Setup(CombatStats combatStats) { }
    public override GameAction GetGameAction() => null;
}