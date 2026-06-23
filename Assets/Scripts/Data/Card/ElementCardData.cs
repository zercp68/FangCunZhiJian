using System.Collections;
using System.Collections.Generic;
using UnityEngine;



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
    Earth ,
    None,
}


[CreateAssetMenu(fileName = "NewElementCardData", menuName = "Data/Card/Element")]
public class ElementCardData : CardData
{
    [field: SerializeField] public E_ElementType E_ElementType { get; private set; }
}
