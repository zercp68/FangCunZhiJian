using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///武器卡特有属性
/// </summary>
public class WeaponCard : Card
{
    public int CardSellMoney=>((WeaponCardData)data).CardSellMoney;

    /// <summary>
    /// 是否已经装备
    /// </summary>
    public bool IsWeaponEquipped { get; set; }

    public E_WeaponType E_WeaponType => ((WeaponCardData)data).E_WeaponType;
    /// <summary>
    /// 攻击力加成
    /// </summary>
    public int attackBonus => ((WeaponCardData)data).attackBonus;
    /// <summary>
    /// 防御力加成
    /// </summary>
    public int defenseBonus=>((WeaponCardData)data).defenseBonus;
    /// <summary>
    /// 基础暴击率
    /// </summary>
    public float baseCriRate=>((WeaponCardData)data).baseCriRate;
    /// <summary>
    /// 基础暴击率伤害倍率
    /// </summary>
    public float baseCritDamage => ((WeaponCardData)data).baseCritDamage;
    /// <summary>
    /// 元素效果增幅倍率
    /// </summary>
    public float elementMult => ((WeaponCardData)data).elementMult;

    /// <summary>
    /// 武器牌带有的攻击牌
    /// </summary>
    public List<CardData> Deck => ((WeaponCardData)data).Deck;


    // 武器卡构造函数
    public WeaponCard(WeaponCardData weaponData) : base(weaponData)
    {
        // 可添加武器卡初始化逻辑（如校验卡组有效性）
    }
}
