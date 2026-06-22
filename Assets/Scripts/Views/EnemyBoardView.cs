using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人的插槽
/// </summary>
public class EnemyBoardView : Singleton<EnemyBoardView>
{
    //视图棋盘列表，当前所有敌人
    [SerializeField] private List<Transform> slots;
    [SerializeField] private GameObject RemoveEnemyVFX;
    public List<EnemyView> EnemyViews {  get; private set; }= new List<EnemyView>();
    
    //创造敌人
    public void AddEnemy(EnemyData enemyData)
    {
        //算出当前敌人有多少,然后位置
        Transform slot = slots[EnemyViews.Count];
        EnemyView enemyView =EnemyViewCreator.Instance.CreateEnemyView(enemyData,slot.position,slot.rotation);
        enemyView.transform.SetParent(slot, false);
        EnemyViews.Add(enemyView);
        //播放敌人出现
    }

    public IEnumerator RemoveEnemy(EnemyView enemyView)
    {
        EnemyViews.Remove(enemyView);
        //这里播放敌人死亡动画
        Tween tween = enemyView.transform.DOScale(Vector3.zero, 0.25f);
        yield return tween.WaitForCompletion();
        Destroy(enemyView.gameObject);
        //生成敌人死亡特效
        //然后销毁
    }
}
