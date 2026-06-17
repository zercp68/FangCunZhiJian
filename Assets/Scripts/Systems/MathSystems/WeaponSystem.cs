using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WeaponSystem : Singleton<WeaponSystem>
{
    [SerializeField] private  WeaponCard currentLeftWeapon;
    [SerializeField] private  WeaponCard currentRightWeapon;

    [SerializeField] private WeaponView LeftWeaponView;
    [SerializeField] private WeaponView RightWeaponView;




    #region 计算系统的定义
    [SerializeField] private CombatSystem leftCombatSystem;
    [SerializeField] private CombatSystem rightCombatSystem;
    #endregion



    private void OnEnable()
    {

        ActionSystem.AttachPerformer<EquipWeaponGA>(equipWeaponPerformer);
        ActionSystem.AttachPerformer<WeaponAddElementGA>(WeaponAddElementPerformer);
        ActionSystem.AttachPerformer<WeaponActionGA>(WeaponActionPerformer);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPrePerformer, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostPerformer, ReactionTiming.POST);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<EquipWeaponGA>();
        ActionSystem.DetachPerformer<WeaponAddElementGA>();
        ActionSystem.DetachPerformer<WeaponActionGA>();
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPrePerformer, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostPerformer, ReactionTiming.POST);

    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="weaponCard"></param>
    public void setup()
    {
        if (currentLeftWeapon!=null)
        {

        }
    }

    public bool IsWeaponEquipped(WeaponCard weaponCard)
    {
        if (weaponCard != null)
        {
            switch (weaponCard.E_WeaponHand)
            {
                case E_WeaponHand.Left:
                    return currentLeftWeapon == null;
                case E_WeaponHand.Right:
                    return currentRightWeapon == null;
            }
        }
        Debug.Log("武器牌判断传入为空");
        return false;
    }

    /// <summary>
    /// 装备武器牌
    /// </summary>
    /// <param name="weaponCard"></param>
    /// <returns></returns>
    public IEnumerator equipWeaponPerformer(EquipWeaponGA equipWeaponGA)
    {
        Debug.Log("执行武器");
        switch (equipWeaponGA.E_WeaponHand)
        {
            case E_WeaponHand.Left:
                currentLeftWeapon = equipWeaponGA.WeaponCard;
                if (currentLeftWeapon != null)
                    leftCombatSystem.SetupWeapon(currentLeftWeapon); // 绑定武器到战斗系统
                if (LeftWeaponView != null)
                    yield return LeftWeaponView.creatWeapon(currentLeftWeapon);
                break;
            case E_WeaponHand.Right:
                currentRightWeapon = equipWeaponGA.WeaponCard;
                if (currentRightWeapon != null)
                    rightCombatSystem.SetupWeapon(currentRightWeapon);
                if (RightWeaponView != null)
                    yield return RightWeaponView.creatWeapon(currentRightWeapon);
                break;
        }
    }


    /// <summary>
    /// 攻击牌是否和指定武器类型匹配
    /// </summary>
    public bool IsMatchWeapon(ActionCard actionCard)
    {
        // 动作牌为空直接不匹配
        if (actionCard == null)
            return false;
        WeaponCard weapon=null;
        switch (actionCard.E_WeaponHand)
        {
            case E_WeaponHand.Left:
                weapon = currentLeftWeapon;
                break;
            case E_WeaponHand.Right:
                weapon = currentRightWeapon;
                break;
        }
        // 2. 没武器 → 不能用
        if (weapon == null)
            return false;

        // 3. 核心判断：动作牌是否支持这个武器类型
        return actionCard.e_validWeaponTypes.Contains(weapon.E_WeaponType);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weaponAddElementGA"></param>
    /// <returns></returns>
    public IEnumerator WeaponAddElementPerformer(WeaponAddElementGA weaponAddElementGA)
    {
        switch (weaponAddElementGA.ElementCard.E_WeaponHand)
        {
            case E_WeaponHand.Left:
                leftCombatSystem.AddElementCard(weaponAddElementGA.ElementCard);
                break;
            case E_WeaponHand.Right:
                rightCombatSystem.AddElementCard (weaponAddElementGA.ElementCard);
                break;
        }

        //这里放为卡牌赋能的动画
        yield return null;
    }

    /// <summary>
    /// 启动动作牌。
    /// </summary>
    /// <param name="weaponActionGA"></param>
    /// <returns></returns>
    public IEnumerator WeaponActionPerformer(WeaponActionGA weaponActionGA)
    {
        ActionCard actionCard = weaponActionGA.ActionCard;
        //EnemyView target = weaponActionGA.Target;
        // ========== 核心修复：敌人目标判空 ==========
        EnemyView target = null;
        // 检查单例 + 列表非空 + 第一个元素非空
        if (weaponActionGA.Target != null)
        {
            target = weaponActionGA.Target;
        }
        else
        {
            Debug.LogError("没有可攻击的敌人目标！");
            yield break; // 无目标则终止执行
        }
        // 判断是攻击还是防御动作
        if (actionCard.isDefenseAction)
        {
            // 防御动作：计算防御值并给英雄加护盾
            int defenseValue = 0;
            switch (actionCard.E_WeaponHand)
            {
                case E_WeaponHand.Left:
                    defenseValue = leftCombatSystem.CalculateDefenseValue(actionCard);
                    break;
                case E_WeaponHand.Right:
                    defenseValue = rightCombatSystem.CalculateDefenseValue(actionCard);
                    break;
            }
            HeroSystem.Instance.HeroView.UpdateShield(defenseValue);
            yield return null;
        }
        else
        {
            Debug.Log(actionCard.E_WeaponHand);
            // 攻击动作
            switch (actionCard.E_WeaponHand)
            {
                case E_WeaponHand.Left:
                    yield return leftCombatSystem.ExecuteAttack(actionCard, target);
                    break;
                case E_WeaponHand.Right:
                    yield return rightCombatSystem.ExecuteAttack(actionCard, target);
                    break;
            }
        }
    }

    /// <summary>
    /// 获取敌人对玩家的攻击伤害值
    /// </summary>
    private int GetEnemyAttackDamage(CombatantView target)
    {
        if (target is EnemyView enemy)
        {
            // 使用 EnemySystem 中已有的伤害计算方法
            return EnemySystem.Instance.GetEnemyDamage(enemy.AttackPower);
        }
        return 5; // 默认伤害
    }

    private void EnemyTurnPrePerformer(EnemyTurnGA enemyTurnGA)
    {
        // 敌人回合开始前（即玩家回合结束），清空所有临时状态，避免残留
        
        leftCombatSystem.ResetTurnState();
        rightCombatSystem.ResetTurnState();
    }

    private void EnemyTurnPostPerformer(EnemyTurnGA enemyTurnGA)
    {
        // 敌人回合结束后，新玩家回合开始时重置（再次确保）
        leftCombatSystem.ResetTurnState();
        rightCombatSystem.ResetTurnState();
    }



}
