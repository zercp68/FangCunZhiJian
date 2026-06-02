using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponActionGA : GameAction
{
    public ActionCard ActionCard { get; private set; }
    public EnemyView Target { get; private set; }
    public WeaponActionGA(ActionCard actionCard, EnemyView target)
    {
        ActionCard = actionCard;
        Target = target;
    }
}
