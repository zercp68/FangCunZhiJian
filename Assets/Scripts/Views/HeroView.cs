using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HeroView : CombatantView
{
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public int currentShield { get; private set; }
    public void Setup(HeroData heroData)
    {
        SetupBase(heroData.Health, heroData.sprite);
    }
    public override void Damage(int damageAmount)
    {
        //1. 先应用防御减免/免疫/反弹（新增）
        damageAmount = HeroSystem.Instance.ApplyDefenseBeforeDamage(damageAmount);

        // 如果减免后伤害为0，直接返回（不触发护盾和掉血）
        if (damageAmount <= 0) return;

        // 2. 再处理护盾抵扣
        int remainingDamage = damageAmount;
        if (currentShield > 0)
        {
            int shieldUsed = Mathf.Min(currentShield, remainingDamage);
            currentShield -= shieldUsed;
            remainingDamage -= shieldUsed;
        }

        // 3. 最后调用基类扣血
        base.Damage(remainingDamage);
    }
    public void ClearShield()
    {
        currentShield = 0;
    }
    public void UpdateShield(int amount)
    {
        currentShield += amount;
        // 更新 UI 显示护盾值
    }

}
