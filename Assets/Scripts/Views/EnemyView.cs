using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyView : CombatantView
{
    [SerializeField] private Text attackText;
    public int AttackPower { get; set; }
    public int DefensePower { get; set; }

    //新增：灼烧相关
    public int BurnStacks { get; set; }          // 当前灼烧层数
    public int BurnRemainingTurns { get; set; }  // 剩余持续回合数
    public void Setup(EnemyData enemyData)
    {
        AttackPower =enemyData.AttackPower;
        UpdateAttackText();
        SetupBase(enemyData.Health, enemyData.sprite);

        BurnStacks = 0;
        BurnRemainingTurns = 0;
    }

    public override void Damage(int damageAmount)
    {
        base.Damage(damageAmount);
    }

    /// <summary>
    /// 更新敌人的攻击力
    /// </summary>
    private void UpdateAttackText()
    {
        attackText.text = "Atk:" + AttackPower;
    }

    /// <summary>
    /// 更新敌人的防御力
    /// </summary>
    private void UpdateDefenseText()
    {
        attackText.text = "Def:" + DefensePower;
    }
}
