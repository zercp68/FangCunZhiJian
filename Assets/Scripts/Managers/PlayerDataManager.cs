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
    [SerializeField] private List<WeaponCardData> initialWeaponDatas;   // 初始卡组（ScriptableObject列表）

    // 运行时数据
    private int currentMoney;
    private List<Card> currentDeck = new List<Card>();   // 当前卡组（可变）
    private List<WeaponCard> currentWeaponDeck = new List<WeaponCard>();

    // 公共只读属性
    public int Money => currentMoney;


    public System.Action<int> OnMoneyChanged;   // 金币变化事件


    private void Start()
    {
        InitNewGame();
        UIManager.Instance.ShowPanel<SynthesisPanel>();
    }

    /// <summary>
    /// 初始化新游戏（重置数据）
    /// </summary>
    public void InitNewGame()
    {
        currentMoney = initialMoney;
        CardManager.Instance.fillCardList(initialCardDatas, currentDeck);
        CardManager.Instance.fillCardList(initialWeaponDatas, currentWeaponDeck);
        // 重置武器
        currentLeftWeapon = null;
        currentRightWeapon = null;
        OnMoneyChanged?.Invoke(currentMoney);   // 添加这一行
        Debug.Log($"初始化完成，金币: {currentMoney}, 卡组数量: {currentDeck.Count}");
        // 新增：检查initialCardDatas是否配置
        if (initialCardDatas == null || initialCardDatas.Count == 0)
        {
            Debug.LogError("PlayerDataManager: initialCardDatas（初始卡组）未在Inspector配置！");
        }
        if (initialWeaponDatas == null || initialWeaponDatas.Count == 0)
        {
            Debug.LogWarning("PlayerDataManager: initialWeaponDatas（初始武器卡组）未配置！");
        }
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
        Debug.Log($"添加卡牌: {card.CardId}，当前卡组数量: {currentDeck.Count}");
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
        if (removed) Debug.Log($"移除卡牌: {card.CardId}");
        return removed;
    }


    #region 装备武器方法
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
        currentLeftWeapon = null;
    }

    public void UnequipRightWeapon() 
    {
        currentRightWeapon = null; 
    }
    /// <summary>
    /// 获取玩家当前已装备的左右手武器
    /// </summary>
    /// <param name="left">输出：左手武器</param>
    /// <param name="right">输出：右手武器</param>
    public void GetEquippedWeapons(out WeaponCard left, out WeaponCard right)
    {
        left = currentLeftWeapon;
        right = currentRightWeapon;
    }
    #endregion

    #region 武器卡组 相关方法
    /// <summary> 获取武器卡组副本（防止外部篡改原集合） </summary>
    public List<WeaponCard> GetCurrentWeaponDeckCopy()
    {
        return new List<WeaponCard>(currentWeaponDeck);
    }

    /// <summary> 单张添加武器卡牌 </summary>
    public void AddCardToWeaponDeck(WeaponCard weaponCard)
    {
        if (weaponCard == null)
        {
            Debug.LogWarning("添加的武器卡牌为空");
            return;
        }
        currentWeaponDeck.Add(weaponCard);
        Debug.Log($"添加武器卡牌: {weaponCard.CardId}，当前武器卡组数量: {currentWeaponDeck.Count}");
    }

    /// <summary> 批量添加武器卡牌 </summary>
    public void AddCardsToWeaponDeck(List<WeaponCard> weaponCards)
    {
        if (weaponCards == null || weaponCards.Count == 0)
        {
            Debug.LogWarning("批量添加的武器卡牌列表为空");
            return;
        }
        currentWeaponDeck.AddRange(weaponCards);
        Debug.Log($"批量添加武器卡牌完成，当前武器卡组数量: {currentWeaponDeck.Count}");
    }

    /// <summary> 移除单张武器卡牌 </summary>
    public bool RemoveCardFromWeaponDeck(WeaponCard weaponCard)
    {
        if (weaponCard == null)
        {
            Debug.LogWarning("要移除的武器卡牌为空");
            return false;
        }
        bool removed = currentWeaponDeck.Remove(weaponCard);
        if (removed)
            Debug.Log($"移除武器卡牌: {weaponCard.CardId}");
        else
            Debug.LogWarning($"武器卡组中未找到卡牌: {weaponCard.CardId}");
        return removed;
    }

    /// <summary> 清空武器卡组 </summary>
    public void ClearWeaponDeck()
    {
        currentWeaponDeck.Clear();
        Debug.Log("武器卡组已清空");
    }

    public void SellWeaponCard(WeaponCard weaponCard)
    {
        AddMoney(weaponCard.CardSellMoney);
        return;
    }
    #endregion
}