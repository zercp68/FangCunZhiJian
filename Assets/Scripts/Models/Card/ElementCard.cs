using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementCard : Card
{
    // 元素卡特有属性
    public E_ElementCardLevel ElementCardLevel => ((ElementCardData)data).E_ElementCardLevel;
    public E_ElementType E_ElementType => ((ElementCardData)data).E_ElementType;

    // 元素卡构造函数
    public ElementCard(ElementCardData elementData) : base(elementData)
    {
        // 可添加元素卡初始化逻辑（如等级关联效果）
    }
}
