using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
    private List<Card> deckCards;
    [SerializeField] private List<EnemyData> enemyDatas;
    [SerializeField] private HeroData heroData;
    private WeaponCard leftWeaCard;
    private WeaponCard RightWeaCard;

    //private void Start()
    //{

    //    EnemySystem.Instance.Setup(enemyDatas);
    //    CardSystem.Instance.Setup(deckData);
    //    DrawCardsGA drawCardsGA = new(6);
    //    ActionSystem.Instance.Perform(drawCardsGA);
    //}

    

    private void Start()
    {
        setupWeapon();
        // 1. 初始化deckCards（防止首次使用时为null）
        if (deckCards == null)
        {
            deckCards = new List<Card>();
        }
        else
        {
            // 2. 清空旧数据（避免叠加）
            deckCards.Clear();
        }
        // 3. 安全获取当前卡组副本并赋值（增加空引用检查）
        if (PlayerDataManager.Instance != null)
        {
            List<Card> deckCopy = PlayerDataManager.Instance.GetCurrentDeckCopy();
            deckCards.AddRange(deckCopy); // 赋值核心逻辑（也可直接 deckCards = deckCopy;）
            Debug.Log($"MatchSetup: 从PlayerDataManager获取到卡组数量: {deckCopy.Count}"); // 新增日志
        }
        else
        {
            Debug.LogError("PlayerDataManager.Instance 为空，请检查是否挂载该单例！");
        }
        // 初始化不受动作系统影响的部分
        EnemySystem.Instance.Setup(enemyDatas);
        HeroSystem.Instance.Setup(heroData);
        Debug.Log($"MatchSetup: 传给CardSystem的卡组数量: {deckCards.Count}"); // 新增日志
        CardSystem.Instance.Setup(deckCards);
        ManaSystem.Instance.Setup();
        StaminaSystem.Instance.Setup();
        // 1. 先抽卡
        StartCoroutine(InitAll());
    }
    private void setupWeapon()
    {
        // 先清空本局旧武器，防止残留
        leftWeaCard = null;
        RightWeaCard = null;

        // 从全局玩家数据读取武器，赋值给当前对局变量
        if (PlayerDataManager.Instance != null)
        {
            // 调用读取方法
            PlayerDataManager.Instance.GetEquippedWeapons(out leftWeaCard, out RightWeaCard);

            string leftName = leftWeaCard == null ? "无" : leftWeaCard.CardId.ToString();
            string rightName = RightWeaCard == null ? "无" : RightWeaCard.CardId.ToString();
            Debug.Log($"MatchSetup：读取玩家武器 → 左手：{leftName}，右手：{rightName}");
        }
        else
        {
            Debug.LogError("PlayerDataManager 单例不存在，无法读取武器！");
        }
    }
    private IEnumerator InitAll()
    {
        // 装备左手武器
        if (leftWeaCard != null)
        {
            EquipWeaponGA setWeaponGA = new(leftWeaCard, E_WeaponHand.Left);
            ActionSystem.Instance.Perform(setWeaponGA);
            while (ActionSystem.Instance.IsBusy) yield return null;
        }
        else
        {
            Debug.Log("左手无武器，跳过装备");
        }

        // 装备右手武器
        if (RightWeaCard != null)
        {
            EquipWeaponGA setWeaponGA = new(RightWeaCard, E_WeaponHand.Right);
            ActionSystem.Instance.Perform(setWeaponGA);
            while (ActionSystem.Instance.IsBusy) yield return null;
        }
        else
        {
            Debug.Log("右手无武器，跳过装备");
        }

        // 后续抽卡逻辑不变
        DrawCardsGA drawCardsGA = new(6);
        ActionSystem.Instance.Perform(drawCardsGA);
        while (ActionSystem.Instance.IsBusy) yield return null;
    }


}
