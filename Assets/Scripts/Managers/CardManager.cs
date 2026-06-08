using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : Singleton<CardManager>
{
    

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
                Debug.Log($"当前是行动卡数据：{actionData.CardName}");
            }
            else if (data is ElementCardData elementData)
            {
                Debug.Log($"当前是元素卡数据：{elementData.CardName}");
            }
            else if (data is WeaponCardData weaponData)
            {
                Debug.Log($"当前是武器卡数据：{weaponData.CardName}");
            }
            else
            {
                Debug.Log($"当前是基础卡牌数据：{data.CardName}");
            }

            // 统一添加（只在这里加一次）
            targetDataList.Add(data);
        }
    }
}
