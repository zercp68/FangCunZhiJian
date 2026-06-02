using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatGA : GameAction
{
    // 当前传递计算的武器卡
    public WeaponCard CurrentWeaponCard { get; private set; }

    // 本回合已打出的属性牌（按顺序）
    private List<ElementCard> tempElementCards = new();

    public  CombatGA(WeaponCard weaponCard)
    {
        CurrentWeaponCard = weaponCard;
    }

}
