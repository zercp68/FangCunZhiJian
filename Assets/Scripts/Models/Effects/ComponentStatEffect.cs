using UnityEngine;

/// <summary>
/// 部件牌：固定攻击力 +1
/// </summary>
public class ComponentStatEffect : Effect
{
    public int attackBonus = 1;

    public override void Setup(CombatStats combatStats)
    {
        // 部件牌不经过 CombatStats，直接由 CombatSystem.AddComponentCard 处理
        // 所以 Setup 留空
    }

    public override GameAction GetGameAction() => null;
}