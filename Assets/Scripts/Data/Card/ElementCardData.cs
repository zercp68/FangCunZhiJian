using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 属性等级
/// </summary>
public enum E_ElementCardLevel
{
    One,
    Two,
    Three,
    Four,
}

/// <summary>
/// 属性类型
/// </summary>
public enum E_ElementType 
{ 
    /// <summary>
    /// 金
    /// </summary>
    Metal, 
    /// <summary>
    /// 木
    /// </summary>
    Wood, 
    /// <summary>
    /// 水
    /// </summary>
    Water, 
    /// <summary>
    /// 火
    /// </summary>
    Fire, 
    /// <summary>
    /// 土
    /// </summary>
    Earth 
}


[CreateAssetMenu(fileName = "NewElementCardData", menuName = "Data/Card/Element")]
public class ElementCardData : CardData
{
    /// <summary>
    /// 元素牌等级
    /// </summary>
    [field: SerializeField] public E_ElementCardLevel E_ElementCardLevel { get; private set; }
    [field: SerializeField] public E_ElementType E_ElementType { get; private set; }
}
