using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SerializeReferenceEditor;
using System.IO;

public enum E_WeaponHand
{
    /// <summary>
    /// 空
    /// </summary>
    None,
    /// <summary>
    /// 赋能左手武器
    /// </summary>
    Left,
    /// <summary>
    /// 赋能右手武器
    /// </summary>
    Right,
}
public enum E_CardType
{
    /// <summary>
    /// 行为卡牌
    /// </summary>
    Action,
    /// <summary>
    /// 属性卡牌
    /// </summary>
    Element,
    /// <summary>
    /// 武器卡牌
    /// </summary>
    weapon
}

[CreateAssetMenu(fileName ="NewCardData",menuName ="Data/Card")]
public class CardData : ScriptableObject
{
    /// <summary>
    /// 卡牌名字
    /// </summary>
    [field:SerializeField]public string CardName { get;  set; }
    /// <summary>
    /// 卡牌类型
    /// </summary>
    [field:SerializeField]public E_CardType CardType { get; set; }
    /// <summary>
    /// 卡牌描述
    /// </summary>
    [field:SerializeField]public string Description {  get; set; }
    /// <summary>
    /// 卡牌消耗法力值
    /// </summary>
    [field:SerializeField]public int manaCost {  get;private set; }
    /// <summary>
    /// 图片
    /// </summary>
    [field:SerializeField]public Sprite Image {  get;private set; }
    /// <summary>
    /// 对左手还是右手的武器赋能
    /// </summary>
    public E_WeaponHand E_WeaponHand;
    /// <summary>
    /// 效果，数值表
    /// </summary>
    [field: SerializeReference, SR] public List<Effect> Effects {  get; private set; }
    

}
