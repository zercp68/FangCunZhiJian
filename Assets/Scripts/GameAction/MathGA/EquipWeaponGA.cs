using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipWeaponGA : GameAction
{
    public WeaponCard WeaponCard { get; set; }
    public E_WeaponHand E_WeaponHand { get; set; }
    public EquipWeaponGA(WeaponCard weaponCard,E_WeaponHand e_WeaponHand) 
    {
        this.WeaponCard = weaponCard;
        this.E_WeaponHand = e_WeaponHand;
    }
}
