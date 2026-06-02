using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CombatResult
{
    /// <summary>
    /// 物理伤害
    /// </summary>
    public int PhysicalDamage;
    /// <summary>
    /// 灼烧伤害
    /// </summary>
    public int BurnDamage;
    /// <summary>
    /// 回血值
    /// </summary>
    public int HealAmount;
    /// <summary>
    /// 给敌人的灼烧层数
    /// </summary>
    public int BurnStacks;   // 施加给敌人的灼烧层数

    public bool IsHit;
    public bool IsCrit;
}
