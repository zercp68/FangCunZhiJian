using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器类型
/// </summary>
public enum E_WeaponType 
{ 
    /// <summary>
    /// 刀
    /// </summary>
    Knife, 
    /// <summary>
    /// 盾
    /// </summary>
    Shield,
    /// <summary>
    /// 弓
    /// </summary>
    bow,
    /// <summary>
    /// 斧
    /// </summary>
    axe,
    /// <summary>
    /// 剑
    /// </summary>
    sword
}

[CreateAssetMenu(fileName ="NewWeaponCardData",menuName = "Data/Card/Weapon")]
public class WeaponCardData : CardData
{
    [field: SerializeField]public int CardSellMoney {  get;private set; }

    /// <summary>
    /// 武器类型
    /// </summary>
    [field: SerializeField] public E_WeaponType E_WeaponType { get; private set; }


    /// <summary>
    /// 攻击力加成
    /// </summary>
    [field: SerializeField] public int attackBonus { get; private set; }
    /// <summary>
    /// 防御力加成
    /// </summary>
    [field: SerializeField] public int defenseBonus { get; private set; }
    /// <summary>
    /// 基础暴击率
    /// </summary>
    [field: SerializeField] public float baseCriRate { get; private set; }
    /// <summary>
    /// 基础暴击率伤害倍率
    /// </summary>
    [field: SerializeField] public float baseCritDamage { get; private set; }
    /// <summary>
    /// 元素效果增幅倍率
    /// </summary>
    [field: SerializeField] public float elementMult { get; private set; }

    /// <summary>
    /// 带有的攻击牌
    /// </summary>
    [field: SerializeField] public List<CardData> Deck { get; set; }


}
