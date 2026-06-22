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
        // 先扣护盾
        int remainingDamage = damageAmount;
        if (currentShield > 0)
        {
            int shieldUsed = Mathf.Min(currentShield, remainingDamage);
            currentShield -= shieldUsed;
            remainingDamage -= shieldUsed;
        }

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
