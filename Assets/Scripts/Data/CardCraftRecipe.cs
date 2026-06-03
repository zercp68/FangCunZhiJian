using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "CardCraftRecipe", menuName = "卡牌/合成配方")]
public class CardCraftRecipe : ScriptableObject
{
    public List<CardData> inputCards; //消耗素材卡牌
    public CardData outputCard; //产出卡牌
}
