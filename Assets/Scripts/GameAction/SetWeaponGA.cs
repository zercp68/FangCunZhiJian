using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetWeaponGA : GameAction
{
    public WeaponCard WeaponCard { get; set; }
    public SetWeaponGA(WeaponCard weaponCard) 
    {
        this.WeaponCard = weaponCard;   
    }
}
