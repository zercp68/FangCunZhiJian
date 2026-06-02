using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCardsGA : GameAction
{
    /// <summary>
    /// 要抽取的牌数
    /// </summary>
    public int Amount {  get; set; }
    public DrawCardsGA(int amount) {  Amount = amount; }
}
