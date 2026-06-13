using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipWeaponGA : GameAction
{
    public WeaponCard WeaponCard { get; set; }
    public EquipWeaponGA(WeaponCard weaponCard) 
    {
        this.WeaponCard = weaponCard;   
    }
}
