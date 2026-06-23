using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchSetupSystem : Singleton<MatchSetupSystem>
{
    private List<Card> deckCards;
    private List<EnemyData> enemyDatas;
    private HeroData heroData;
    [SerializeField] private Image imgBK;

    public IEnumerator SetupCoroutine(Level level)
    {
        if (level == null)
        {
            Debug.LogError("传入的 Level 为空！");
            yield break;
        }

        setupLevelData(level);
        setupDeckCards();

        EnemySystem.Instance.Setup(enemyDatas);
        HeroSystem.Instance.Setup(heroData);

        // 武器装备（异步等待）
        WeaponSystem.Instance.setup();
        while (!WeaponSystem.Instance.IsReady)
        {
            yield return null;
        }

        CardSystem.Instance.Setup(deckCards);
        ManaSystem.Instance.Setup();
        StaminaSystem.Instance.Setup();

        // 抽卡
        yield return InitAll();
    }

    private void setupLevelData(Level level)
    {
        deckCards = new List<Card>();
        enemyDatas = new List<EnemyData>();
        heroData = new HeroData();
        if (level.enemyDatas != null)
        {
            enemyDatas.AddRange(level.enemyDatas);
        }
        if (level.heroData != null)
        {
            heroData = level.heroData;
        }
        if (level.imgBK != null && level.imgBK.sprite != null)
        {
            imgBK.sprite = level.imgBK.sprite;
        }
    }

    void setupDeckCards()
    {         // 1. 初始化deckCards（防止首次使用时为null）
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
            List<Card> deckCopy = PlayerDataManager.Instance.GetCurrentHandDeckCopy();
            deckCards.AddRange(deckCopy); // 赋值核心逻辑（也可直接 deckCards = deckCopy;）
            Debug.Log($"MatchSetup: 从PlayerDataManager获取到卡组数量: {deckCopy.Count}"); // 新增日志
        }
        else
        {
            Debug.LogError("PlayerDataManager.Instance 为空，请检查是否挂载该单例！");
        }
    }
    private IEnumerator InitAll()
    {
        // 后续抽卡逻辑不变
        DrawCardsGA drawCardsGA = new(6);
        ActionSystem.Instance.Perform(drawCardsGA);
        while (ActionSystem.Instance.IsBusy) yield return null;
    }


}
