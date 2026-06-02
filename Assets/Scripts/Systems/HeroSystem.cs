using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSystem : Singleton<HeroSystem>
{
    [field:SerializeField]public HeroView HeroView { get;private set; }
    private int currentShield = 0;  // 临时护盾值
    public void Setup(HeroData heroData)
    {
        HeroView.Setup(heroData);
    }
    public int Damage(int damageAmount)
    {
        // 先扣护盾
        int remainingDamage = damageAmount;
        if (currentShield > 0)
        {
            int shieldUsed = Mathf.Min(currentShield, remainingDamage);
            currentShield -= shieldUsed;
            remainingDamage -= shieldUsed;
        }
        if (remainingDamage > 0)
        {
            return remainingDamage;
        }
        else
        {
            return 0;
        }
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
