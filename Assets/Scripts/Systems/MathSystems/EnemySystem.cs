using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class EnemySystem : Singleton<EnemySystem>
{
    [SerializeField]private EnemyBoardView enemyBoardView;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
        ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroPerformer);
        ActionSystem.AttachPerformer<KillEnemyGA>(KillEnemyPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<AttackHeroGA>();
        ActionSystem.DetachPerformer<KillEnemyGA>();

    }

    public void Setup(List<EnemyData> enemyDatas)
    {
        foreach (EnemyData enemyData in enemyDatas) 
        {
            enemyBoardView.AddEnemy(enemyData);
        }
    }
    
    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGA)
    {
        foreach (var enemy in enemyBoardView.EnemyViews)
        {
            AttackHeroGA attackHeroGA = new(enemy);
            ActionSystem.Instance.AddReaction(attackHeroGA);
        }
        yield return null;
    }

    /// <summary>
    /// 敌人攻击英雄动画
    /// </summary>
    /// <param name="enemyTurnGA"></param>
    /// <returns></returns>
    private IEnumerator AttackHeroPerformer(AttackHeroGA attackHeroGA) 
    {
        EnemyView attacker = attackHeroGA.Attacker;
        Vector3 originPos = attacker.transform.position; // 保存攻击前原位
                                                         //这里放触发动画
        Tween forwardTween = attacker.transform.DOMoveX(originPos.x - 1f, 0.15f);
        yield return forwardTween.WaitForCompletion();
        // 退回原始位置
        Tween backTween = attacker.transform.DOMoveX(originPos.x, 0.05f);
        yield return backTween.WaitForCompletion();

        //对英雄造成伤害
        int damage = GetEnemyDamage(attacker.AttackPower);
        Debug.Log("对英雄造成" +damage);
        DealDamageGA dealDamageGA = new(damage, new() { HeroSystem.Instance.HeroView});
        ActionSystem.Instance.AddReaction(dealDamageGA);

        // 4. 修正 WaitForSeconds
        yield return new WaitForSeconds(0.15f);
    }

    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {
        yield return enemyBoardView.RemoveEnemy(killEnemyGA.EnemyView);
    }

    #region 计算敌人对英雄的伤害
    /// <summary>
    /// 计算敌人对你的最终伤害（自带随机浮动 + 随机暴击）
    /// </summary>
    /// <param name="baseAttack">敌人基础攻击（只传这一个）</param>
    /// <returns>最终伤害</returns>
    public int GetEnemyDamage(int baseAttack)
    {
        // 1. 伤害随机浮动：基础攻击 ±10%（你可以改幅度）
        float min = baseAttack * 0.9f;
        float max = baseAttack * 1.1f;
        int Damage = Mathf.RoundToInt(Random.Range(min, max));

        // 2. 敌人暴击率：15% 概率暴击（你可以随便改）
        float critRate = 0.15f;
        bool isCrit = Random.value < critRate;

        // 3. 暴击 ×1.5 倍
        if (isCrit)
        {
            Damage = Mathf.RoundToInt(Damage * 1.5f);
        }
        int finalDamage = Mathf.Max(1, Damage);
        // 确保伤害不会变成 0
        return finalDamage;
    }
    #endregion
}
