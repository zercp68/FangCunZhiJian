using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card
{

    protected readonly CardData data;
    public CardData CardData => data;
    public int CardId => data.cardId;
    public E_CardType E_CardType => data.CardType;
    public string Description => data.Description;
    public Sprite Image => data.Image;



    public E_WeaponHand E_WeaponHand { get; set; }
    public List<Effect> Effects => data.Effects;
    public int ManaCost {  get;private set; }
    public int CardMoney { get; set; }

    public Card(CardData cardData) 
    {
        this.data = cardData;
        this.ManaCost = cardData.manaCost;
        this.CardMoney = cardData.CardMoney;
        this.E_WeaponHand= cardData.E_WeaponHand;
    }
}
