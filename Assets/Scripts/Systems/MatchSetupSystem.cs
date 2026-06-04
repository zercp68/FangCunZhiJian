using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
    private List<Card> deckCards;
    [SerializeField] private List<EnemyData> enemyDatas;
    [SerializeField] private WeaponCardData weaponData;
    //private void Start()
    //{

    //    EnemySystem.Instance.Setup(enemyDatas);
    //    CardSystem.Instance.Setup(deckData);
    //    DrawCardsGA drawCardsGA = new(6);
    //    ActionSystem.Instance.Perform(drawCardsGA);
    //}

    

    private void Start()
    {
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
        Debug.Log($"MatchSetup: 传给CardSystem的卡组数量: {deckCards.Count}"); // 新增日志
        CardSystem.Instance.Setup(deckCards);
        // 1. 先抽卡
        StartCoroutine(InitAll());
    }

    private IEnumerator InitAll()
    {
        DrawCardsGA drawCardsGA = new(6);
        ActionSystem.Instance.Perform(drawCardsGA);
        while (ActionSystem.Instance.IsBusy) yield return null;

        WeaponCard weaponCard = WeaponSystem.Instance.Setup(weaponData);
        SetWeaponGA setWeaponGA = new(weaponCard);
        ActionSystem.Instance.Perform(setWeaponGA);
        while (ActionSystem.Instance.IsBusy) yield return null;

    }
}
