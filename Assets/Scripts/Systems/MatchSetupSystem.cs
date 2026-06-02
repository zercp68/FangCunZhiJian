using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
    [SerializeField] private List<CardData> deckData;
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
        // 初始化不受动作系统影响的部分
        EnemySystem.Instance.Setup(enemyDatas);
        CardSystem.Instance.Setup(deckData);

        // 用协程 分步执行，避免同一帧拥挤
        StartCoroutine(InitHandCard());
        StartCoroutine(InitWeapon());
    }

    private IEnumerator InitHandCard()
    { 
        // 1. 先抽卡
        DrawCardsGA drawCardsGA = new(6);
        ActionSystem.Instance.Perform(drawCardsGA);

        // 等待一帧，让抽卡执行完
        yield return new WaitForSeconds(1f);


    }

    private IEnumerator InitWeapon()
    {
        // 2. 再装备武器（这时候系统一定不忙）
        WeaponCard weaponCard = WeaponSystem.Instance.Setup(weaponData);
        SetWeaponGA setWeaponGA = new(weaponCard);
        ActionSystem.Instance.Perform(setWeaponGA);
        yield return new WaitForSeconds(1f);
    }
}
