using System.Collections;
using UnityEngine;

public class HealHeroGA : GameAction
{
    public int Amount { get; private set; }

    public HealHeroGA(int amount)
    {
        Amount = amount;
    }
}