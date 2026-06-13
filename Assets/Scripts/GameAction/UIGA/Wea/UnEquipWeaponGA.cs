using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnEquipWeaponGA : GameAction
{
    public E_WeaponHand E_WeaponHand { get; private set; }
    public WeaponCard WeaponCard { get; private set; }
    public UnEquipWeaponGA(E_WeaponHand e_WeaponHand,WeaponCard weaponCard=null)
    {
        this.E_WeaponHand = e_WeaponHand;
        this.WeaponCard = weaponCard;
    }
}
