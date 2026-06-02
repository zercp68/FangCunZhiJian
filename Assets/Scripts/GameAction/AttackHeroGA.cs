using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人攻击行为
/// 传敌人EnemyView
/// </summary>
public class AttackHeroGA : GameAction
{
    public EnemyView Attacker {  get; private set; }
    public AttackHeroGA(EnemyView attacker)
    {
        this.Attacker = attacker;
    }
}
