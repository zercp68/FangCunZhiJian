using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAddElementGA : GameAction
{
    public ElementCard ElementCard;
    public WeaponAddElementGA(ElementCard elementCard)
    {
        ElementCard = elementCard;
    }
}
