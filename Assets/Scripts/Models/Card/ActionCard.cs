using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCard : Card
{
    /// <summary>
    /// 适应的武器类型
    /// </summary>
    public List<E_WeaponType> e_validWeaponTypes => ((ActionCardData)data).E_validWeaponTypes;

    /// <summary>
    ///true = 防御动作，false = 攻击动作
    /// </summary>
    public bool isDefenseAction => ((ActionCardData)data).isDefenseAction;
    // 行动卡构造函数
    public ActionCard(ActionCardData actionData) : base(actionData)
    {
        // 可添加行动卡初始化逻辑（如绑定行动执行逻辑）
    }
}
