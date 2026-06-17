using UnityEngine;

/// <summary>
/// 固定额外回血（四木/四水）
/// </summary>
public class ExtraHealEffect : Effect
{
    [Tooltip("额外回复失去生命的比例（如 0.5 表示 50%）")]
    public int extraHealFromLost;

    public override void Setup(CombatStats combatStats)
    {
        combatStats.extraHealFromLost += extraHealFromLost;
    }

    public override GameAction GetGameAction() => null;
}