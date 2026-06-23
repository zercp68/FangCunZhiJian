using UnityEngine;

/// <summary>
/// 伤害减免效果（百分比）
/// </summary>
public class DamageReductionEffect : Effect
{
    public float reductionPercent;  // 0.3 = 30%

    public override void Setup(CombatStats combatStats) { /* 暂不处理 */ }
    public override GameAction GetGameAction() => null;
}