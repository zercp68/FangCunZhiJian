using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
    [SerializeField] private List<Card> deckCards;
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
        deckCards = PlayerDataManager.Instance.GetCurrentDeckCopy();
        // 初始化不受动作系统影响的部分
        EnemySystem.Instance.Setup(enemyDatas);
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
