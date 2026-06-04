using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // 用于 ToList

public class PlayerDataManager : Singleton<PlayerDataManager>
{
    // PlayerDataManager.cs 新增字段
    [Header("当前装备武器")]
    [SerializeField] private WeaponCard currentLeftWeapon;
    [SerializeField] private WeaponCard currentRightWeapon;

    // 公共只读/写入属性
    public WeaponCard CurrentLeftWeapon => currentLeftWeapon;
    public WeaponCard CurrentRightWeapon => currentRightWeapon;

    [Header("初始配置")]
    [SerializeField] private int initialMoney = 100;
    [SerializeField] private List<CardData> initialCardDatas;   // 初始卡组（ScriptableObject列表）

    // 运行时数据
    private int currentMoney;
    private List<Card> currentDeck = new List<Card>();   // 当前卡组（可变）

    // 公共只读属性
    public int Money => currentMoney;


    public System.Action<int> OnMoneyChanged;   // 金币变化事件

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


    private void Start()
    {
        InitNewGame();
    }

    /// <summary>
    /// 初始化新游戏（重置数据）
    /// </summary>
    public void InitNewGame()
    {
        currentMoney = initialMoney;
        fillCardList(initialCardDatas, currentDeck);
        // 重置武器
        currentLeftWeapon = null;
        currentRightWeapon = null;
        OnMoneyChanged?.Invoke(currentMoney);   // 添加这一行
        Debug.Log($"初始化完成，金币: {currentMoney}, 卡组数量: {currentDeck.Count}");

    }

    /// <summary>
    /// 增加金币
    /// </summary>
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        Debug.Log($"金币 +{amount}，当前: {currentMoney}");
        // 可触发事件（如果需要）
        OnMoneyChanged?.Invoke(currentMoney);
    }

    /// <summary>
    /// 消费金币
    /// </summary>
    /// <returns>是否成功</returns>
    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            Debug.Log($"金币 -{amount}，当前: {currentMoney}");
            OnMoneyChanged?.Invoke(currentMoney);
            return true;
        }
        else
        {
            Debug.Log($"金币不足！需要 {amount}，当前 {currentMoney}");
            return false;
        }
    }


    /// <summary>
    /// 获取当前卡组（返回副本，避免外部直接修改）
    /// </summary>
    public List<Card> GetCurrentDeckCopy()
    {
        return new List<Card>(currentDeck);
    }

    /// <summary>
    /// 添加卡牌到当前卡组
    /// </summary>
    public void AddCardToDeck(Card card)
    {
        currentDeck.Add(card);
        Debug.Log($"添加卡牌: {card.CardName}，当前卡组数量: {currentDeck.Count}");
    }

    /// <summary>
    /// 添加卡牌链表到当前卡组
    /// </summary>
    public void AddCardsToDeck(List<Card> cards)
    {
        currentDeck.AddRange(cards);
        Debug.Log($"添加卡牌，当前卡组数量: {currentDeck.Count}");
    }

    /// <summary>
    /// 从当前卡组移除卡牌
    /// </summary>
    public bool RemoveCardFromDeck(Card card)
    {
        bool removed = currentDeck.Remove(card);
        if (removed) Debug.Log($"移除卡牌: {card.CardName}");
        return removed;
    }

    // 装备武器的方法
    public void EquipLeftWeapon(WeaponCard weapon)
    {
        currentLeftWeapon = weapon;
        // 可选：触发装备事件，通知 UI 更新
    }

    public void EquipRightWeapon(WeaponCard weapon)
    {
        currentRightWeapon = weapon;
    }

    // 卸下武器的方法
    public void UnequipLeftWeapon()
    {
        AddMoney(currentLeftWeapon.CardSellMoney);
        currentLeftWeapon = null;

    }

    public void UnequipRightWeapon() 
    {
        AddMoney(currentRightWeapon.CardSellMoney);
        currentRightWeapon = null; 
    }
}