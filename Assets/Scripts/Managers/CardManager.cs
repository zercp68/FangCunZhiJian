using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConbimeInfo
{
    public string result;
    public List<string> otherCards;
}

public class CardManager : Singleton<CardManager>
{

    private List<ConbimeInfo> combineCfg = new List<ConbimeInfo>();

    protected override void Awake()
    {
        base.Awake();

        //combineCfg.Add("卡牌1", new List<string>() { "卡牌2", "卡牌3"});

        //foreach (var item in combineCfg)
        //{
        //    item.otherCards
        //}
    }

    [SerializeField] private List<CardData> allCardDatas;


    public void fillCardList(List<CardData> cardDatas, List<Card> targetList)
    {
        targetList.Clear();
        foreach (var cardData in cardDatas)
        {
            Card card;

            //关键：判断数据类型，创建对应的卡牌类型
            if (cardData is ActionCardData actionData)
            {
                // 是行动卡数据 → 创建 ActionCard
                card = new ActionCard(actionData);
            }
            else if (cardData is ElementCardData elementCardData)
            {
                card = new ElementCard(elementCardData);
            }
            else if (cardData is WeaponCardData weaponCardData)
            {
                card = new WeaponCard(weaponCardData);
            }
            else
            {
                Debug.Log("为普通牌，初始化错误，错误");
                // 普通卡 → 创建基础 Card
                card = new Card(cardData);
            }
            targetList.Add(card);
        }
    }
    /// <summary>
    /// 卡牌实例集合 反向转为 CardData 集合（带类型日志）
    /// </summary>
    public void fillCardDataList(List<Card> cardList, List<CardData> targetDataList)
    {
        if (cardList == null || targetDataList == null)
        {
            Debug.LogWarning("传入列表为空，转换终止");
            return;
        }

        targetDataList.Clear();
        foreach (Card card in cardList)
        {
            CardData data = card.CardData;
            if (data == null)
            {
                Debug.LogWarning("卡牌对应的 CardData 为空，跳过");
                continue;
            }

            // 类型判断 + 打印日志
            if (data is ActionCardData actionData)
            {
                Debug.Log($"当前是行动卡数据：{actionData.cardId}");
            }
            else if (data is ElementCardData elementData)
            {
                Debug.Log($"当前是元素卡数据：{elementData.cardId}");
            }
            else if (data is WeaponCardData weaponData)
            {
                Debug.Log($"当前是武器卡数据：{weaponData.cardId}");
            }
            else
            {
                Debug.Log($"当前是基础卡牌数据：{data.cardId}");
            }

            // 统一添加（只在这里加一次）
            targetDataList.Add(data);
        }
    }
    public void SplitCardsByType(List<Card> cards, 
        List<Card> targetActionCards,   
        List<Card> targetElementCards,
        List<Card> targetWeaponCards)
    {
        // 先清空所有目标列表
        targetActionCards?.Clear();
        targetElementCards?.Clear();
        targetWeaponCards?.Clear();

        if (cards == null)
        {
            Debug.LogWarning("传入的卡牌列表为空，无法分类");
            return;
        }

        foreach (var card in cards)
        {
            if (card == null) continue;

            if (card is ActionCard actionCard)
            {
                targetActionCards?.Add(actionCard);
            }
            else if (card is ElementCard elementCard)
            {
                targetElementCards?.Add(elementCard);
            }
            else if (card is WeaponCard weaponCard)
            {
                targetWeaponCards?.Add(weaponCard);
            }
            else
            {
                // 基础 Card 类型（非子类），如果需要可以单独处理
                Debug.Log($"发现基础卡牌类型：{card.CardId}，未分到任何子列表");
            }
        }
    }

    public void fillCardListByCardLogic(List<CardLogic> cardLogics,List<Card> targetList)
    {
        targetList.Clear();
        foreach(var cardLogic in cardLogics)
        {
            targetList.Add(cardLogic.card);
        }
    }
    /// <summary>
    /// 根据卡牌唯一ID 创建卡牌实例
    /// </summary>
    public Card CreateCardById(int cardId)
    {
        var cardData = allCardDatas.FirstOrDefault(d => d.cardId == cardId);
        if (cardData == null) return null;

        // 按类型生成对应卡牌（复用你原有 fillCardList 逻辑）
        Card card;
        if (cardData is ActionCardData actionData)
            card = new ActionCard(actionData);
        else if (cardData is ElementCardData eleData)
            card = new ElementCard(eleData);
        else if (cardData is WeaponCardData weaponData)
            card = new WeaponCard(weaponData);
        else
            card = new Card(cardData);

        return card;
    }
}
