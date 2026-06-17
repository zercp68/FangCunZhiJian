using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyView : CombatantView
{
    [SerializeField] private Text attackText;
    /// <summary>
    /// 敌人攻击力
    /// </summary>
    public int AttackPower { get; set; }
    /// <summary>
    /// 敌人防御力
    /// </summary>
    public int DefensePower { get; set; }
    public void Setup(EnemyData enemyData)
    {
        AttackPower =enemyData.AttackPower;
        UpdateAttackText();
        SetupBase(enemyData.Health, enemyData.sprite);
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
