using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAddConponentGA : GameAction
{
    public ComponentCard componentCard;
    public WeaponAddConponentGA(ComponentCard componentCard)
    {
        this.componentCard = componentCard;
    }
}
