using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeWeaGA : GameAction
{
    public E_WeaponHand E_WeaponHand { get; private set; }
    public ChangeWeaGA(E_WeaponHand e_WeaponHand)
    {
        this.E_WeaponHand = e_WeaponHand;
    }
}
