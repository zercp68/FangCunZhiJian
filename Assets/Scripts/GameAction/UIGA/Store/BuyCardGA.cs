using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyCardGA : GameAction
{
    public Card Card { get; set; }
    public BuyCardGA(Card card)
    {
        Card = card;
    }
}
