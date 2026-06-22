using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对敌人施加持续灼烧效果（每回合扣血）。
/// 需要被 DamageSystem 或新建 BurnSystem 执行。
/// </summary>
public class ApplyBurnGA : GameAction
{
    public EnemyView Target { get; private set; }
    public int Stacks { get; private set; }
    public int Duration { get; private set; } // 新增：持续回合数

    public ApplyBurnGA(EnemyView target, int stacks, int duration)
    {
        Target = target;
        Stacks = stacks;
        Duration = duration;
    }
}
