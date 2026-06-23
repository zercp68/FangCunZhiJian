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


    [SerializeField] private List<CardData> allCardDatas;
    private List<Card> allCards;



    public void GetAllCards(List<Card> cards)
    {
        cards = new List<Card>();
        cards.AddRange(allCards);
    }
    public void setup()
    {
        // 关键：提前实例化 allCards，避免 fillCardList 操作 null 列表
        allCards = new List<Card>();
        fillCardList(allCardDatas, allCards);
    }
    /// <summary>
    /// 泛型：根据CardData列表生成对应Card实例，填充到目标集合
    /// TData : 卡牌数据基类(可传CardData/WeaponCardData等子类)
    /// TCard : 卡牌实体基类(可传Card/WeaponCard等子类)
    /// </summary>
    public void fillCardList<TData, TCard>(List<TData> cardDatas, List<TCard> targetList)
        where TData : CardData    // 约束：必须是CardData及其子类
        where TCard : Card        // 约束：必须是Card及其子类
    {
        if (cardDatas == null)
        {
            Debug.LogError("cardDatas 列表为空！");
            targetList.Clear();
            return;
        }
        if (targetList == null)
        {
            Debug.LogError("targetList 目标集合为空！");
            targetList = new List<TCard>();
        }

        targetList.Clear();

        foreach (var data in cardDatas)
        {
            Card card = null;

            // 根据数据类型 创建对应卡牌实体
            if (data is ActionCardData actionData)
            {
                card = new ActionCard(actionData);
            }
            else if (data is ElementCardData elementData)
            {
                card = new ElementCard(elementData);
            }
            else if (data is WeaponCardData weaponData)
            {
                card = new WeaponCard(weaponData);
            }
            else if (data is ComponentCardData componentCardData)
            {
                card = new ComponentCard(componentCardData);
            }
            else
            {
                // 基础CardData → 基础Card
                card = new Card(data);
                Debug.LogWarning($"未知卡牌类型，使用基础Card实例: {data.name}");
            }

            // 安全强转（泛型约束保证一定能转）
            if (card is TCard tCard)
            {
                targetList.Add(tCard);
            }
            else
            {
                Debug.LogError($"卡牌类型不匹配！预期: {typeof(TCard).Name}，实际: {card.GetType().Name}");
            }
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
        List<Card> targetCombatCards,
        List<Card> targetWeaponCards)
    {
        // 先清空所有目标列表
        targetActionCards?.Clear();
        targetElementCards?.Clear();
        targetCombatCards?.Clear();
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
            else if (card is ComponentCard componentCard)
            {
                targetCombatCards?.Add(componentCard);
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
    public void SplitCardsByRarity(List<Card> Ncards, List<Card> Rcards, List<Card> SRcards, List<Card> SRRcards)
    {
        //先清空外部传入的列表（判空避免空指针）
        Ncards?.Clear();
        Rcards?.Clear();
        SRcards?.Clear();
        SRRcards?.Clear();

        if (allCards == null)
        {
            Debug.LogError("allCards 为空，无法拆分稀有度");
            return;
        }

        foreach (var card in allCards)
        {
            if (card == null) continue; // 跳过空卡牌
            switch (card.E_Rarity)
            {
                case E_Rarity.N:
                    Ncards?.Add(card);
                    break;
                case E_Rarity.R:
                    Rcards?.Add(card);
                    break;
                case E_Rarity.SR:
                    SRcards?.Add(card);
                    break;
                case E_Rarity.SSR:
                    SRRcards?.Add(card);
                    break;
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
