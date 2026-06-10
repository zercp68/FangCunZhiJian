using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 战斗数值核心系统（类）
/// 负责：
/// - 管理当前装备的武器及武器常驻数值
/// - 管理本回合已打出的属性牌序列（顺序连击）
/// - 累加属性牌带来的临时加成（含五行相生翻倍）
/// - 根据动作牌计算最终伤害、暴击、命中、灼烧、回血等
/// - 产生原子 GameAction（伤害、治疗、灼烧等）
/// </summary>
public class CombatSystem
{
    // ======================= 配置数据 =======================

    private float playerBaseHitRate = 0.8f;   // 基础命中率 80%
    private float defaultCritDamage = 1.5f;   // 默认暴击伤害 150%


    [Header("五行相克关系")]
    [Tooltip("Key: 前一个元素, Value:当前元素")]
    public static Dictionary<E_ElementType, E_ElementType> CounterDict = new()
{
    { E_ElementType.Wood, E_ElementType.Metal },   // 金克木
    { E_ElementType.Earth, E_ElementType.Wood },   // 木克土
    { E_ElementType.Water, E_ElementType.Earth },  // 土克水
    { E_ElementType.Fire, E_ElementType.Water },   // 水克火
    { E_ElementType.Metal, E_ElementType.Fire },   // 火克金
};

    [Header("五行相生关系")]
    [Tooltip("Key: 当前元素, Value: 需要的前一个元素（触发连击）")]
    private readonly Dictionary<E_ElementType, E_ElementType> generateMap = new()
    {
        { E_ElementType.Water, E_ElementType.Metal },  // 金 → 水
        { E_ElementType.Wood,  E_ElementType.Water },  // 水 → 木
        { E_ElementType.Fire,  E_ElementType.Wood  },  // 木 → 火
        { E_ElementType.Earth, E_ElementType.Fire  },  // 火 → 土
        { E_ElementType.Metal, E_ElementType.Earth },  // 土 → 金
    };

    // ======================= 运行时状态 =======================
    /// <summary>
    ///当前装备的武器（武器牌只在此处存储，不进手牌）
    /// </summary>
    public WeaponCard CurrentWeapon { get; private set; }

    /// <summary>
    ///本回合已打出的属性牌列表（按顺序）
    /// </summary>
    private List<ElementCard> tempElementCards = new();

    #region  // ----------------- 武器常驻数值 -----------------
    public float BaseAttack { get; private set; } = 5;         //基础伤害值
    public float BaseDefense { get; private set; } = 5;         //基础防御值        
    public float BaseCritRate { get; private set; } = 0f;       
    public float BaseCritDamage { get; private set; } = 1.5f;
    public float BaseHitRate { get; private set; } = 0.8f;
    public float ElementMult { get; private set; } = 1f;   // 剑额外增幅
    #endregion

    #region// ----------------- 临时加成（单回合，属性牌累积） -----------------
    /// <summary>
    /// // 水系列：全属性百分比加成
    /// </summary>
    public float TempAllStatBonus { get; private set; }
    /// <summary>
    /// // 火/土：攻击力百分比加成
    /// </summary>
    public float TempAtkPercentBonus { get; private set; }
    /// <summary>
    ///  // 土系列：固定防御加成
    /// </summary>
    public int TempDefFlat { get; private set; }
    /// <summary>
    /// // 金系列：暴击率加成
    /// </summary>
    public float TempCritRateBonus { get; private set; }
    /// <summary>
    ///  // 未使用（暂留给特殊效果）
    /// </summary>
    public float TempCritDamageBonus { get; private set; }
    /// <summary>
    ///  // 土系列：命中率加成
    /// </summary>
    public float TempHitRateBonus { get; private set; }
    /// <summary>
    /// // 火系列：灼烧层数
    /// </summary>
    public int TempBurnStacks { get; private set; }
    /// <summary>
    /// // 木系列：回血百分比总和
    /// </summary>
    public float TempHealPercentTotal { get; private set; }
    /// <summary>
    /// // 四木特殊：回复失去生命比例
    /// </summary>
    public float TempExtraHealFlat { get; private set; }
    #endregion


    //private void OnEnemyTurnStart(EnemyTurnGA obj)
    //{
    //    // 敌人回合开始前（即玩家回合结束），清空所有临时状态，避免残留
    //    ResetTurnState();
    //}

    //private void OnEnemyTurnEnd(EnemyTurnGA obj)
    //{
    //    // 敌人回合结束后，新玩家回合开始时重置（再次确保）
    //    ResetTurnState();
    //}

    // ======================= 公共方法 =======================

    /// <summary>
    /// 重置本回合所有临时状态（每回合开始前调用）
    /// </summary>
    public void ResetTurnState()
    {
        tempElementCards.Clear();
        TempAllStatBonus = 0f;
        TempAtkPercentBonus = 0f;
        TempDefFlat = 0;
        TempCritRateBonus = 0f;
        TempCritDamageBonus = 0f;
        TempHitRateBonus = 0f;
        TempBurnStacks = 0;
        TempHealPercentTotal = 0f;
        TempExtraHealFlat = 0f;
    }

    /// <summary>
    /// 装备武器（武器牌不进入手牌，直接调用此方法）
    /// </summary>
    public void EquipWeapon(WeaponCard weapon)
    {
        CurrentWeapon = weapon;
        if (CurrentWeapon == null)
        {
            Debug.Log("武器牌未装备成功");
            return;
        }
        // 读取武器数值（你需要根据实际 WeaponCard 的字段调整）
        // 假设 WeaponCard 上有 AttackBonus, DefenseBonus, BaseCritRate, BaseCritDamage, ElementMult
        BaseAttack = 5 + weapon.attackBonus;
        BaseDefense = 5 + weapon.defenseBonus;
        BaseCritRate = weapon.baseCriRate;
        BaseCritDamage = weapon.baseCritDamage != 0 ? weapon.baseCritDamage : defaultCritDamage;
        BaseHitRate = playerBaseHitRate;
        ElementMult = weapon.elementMult;
    }

    /// <summary>
    /// 打出一张属性牌，累加效果（会自动检查与前一张牌的五行相生）
    /// </summary>
    /// <param name="card">当前打出的属性牌</param>
    public void AddElementCard(ElementCard elementCard)
    {
        // 获取前一张牌（用于连击判定）
        ElementCard previous = tempElementCards.Count > 0 ? tempElementCards.Last() : null;
        tempElementCards.Add(elementCard);

        // 获取元素类型和等级
        E_ElementType element = elementCard.E_ElementType;
        E_ElementCardLevel level = elementCard.ElementCardLevel;

        // 获取基础效果数值（根据等级查表）
        CombatStats combatStats=GetBaseEffect(elementCard);


        // 判定是否触发连击（五行相生）
        bool trigger = previous != null && generateMap.ContainsKey(element) && generateMap[element] == previous.E_ElementType;

        // 根据元素类型累加（触发时数值翻倍或特殊处理）
        switch (element)
        {
            case E_ElementType.Metal:
                float finalCrit = combatStats.critRateBonus;
                if (trigger) finalCrit *= 2;
                TempCritRateBonus += finalCrit;
                break;
            case E_ElementType.Wood:
                float finalHeal =combatStats.healPercent;
                if (trigger) finalHeal *= 2;
                TempHealPercentTotal += finalHeal;
                TempExtraHealFlat +=combatStats.extraHealFromLost;   // 四木的额外回血
                break;
            case E_ElementType.Water:
                float finalAllStat =combatStats.allStatBonus;
                if (trigger) finalAllStat *= 2;
                TempAllStatBonus += finalAllStat;
                break;
            case E_ElementType.Fire:
                float finalDmg =combatStats.damagePercentBonus;
                if (trigger) finalDmg *= 2;
                TempAtkPercentBonus += finalDmg;
                int finalBurn =combatStats.burnStacks;
                if (trigger) finalBurn *= 2;
                TempBurnStacks += finalBurn;
                break;
            case E_ElementType.Earth:
                int finalDef = combatStats.defenseFlatBonus;
                if (trigger) finalDef *= 2;
                TempDefFlat += finalDef;
                // 土牌还加命中，但命中率加成比较特殊，这里单独处理
                // 基础效果中土的命中加成是固定的20%/40%/60%/100%（跟随等级）
                float hitBonus =combatStats.hitRateBonus ;
                if (trigger) hitBonus *= 2;
                TempHitRateBonus += hitBonus;
                break;
        }
    }

    /// <summary>
    /// 计算攻击动作的最终结果（并自动生成原子 GameAction）
    /// </summary>
    /// <param name="action">打出的动作牌</param>
    /// <param name="target">攻击目标（可以是多个，但单体先只传一个）</param>
    public IEnumerator ExecuteAttack(ActionCard actionCard, EnemyView target)
    {
        // 1. 从动作牌的 Effects 中提取动作加成（暴击率、暴伤、伤害倍率等）
        float actionCardCritRate = 0f, actionCardCritDmg = 0f, actionCardDmgMult = 1f;
        foreach (var effect in actionCard.Effects)
        {
            if (effect is ActionStatEffect statEffect)
            {
                switch (statEffect.statType)
                {
                    case ActionStatType.CritRateBonus: actionCardCritRate += statEffect.value; break;
                    case ActionStatType.CritDamageBonus: actionCardCritDmg += statEffect.value; break;
                    case ActionStatType.DamageMultiplier: actionCardDmgMult *= statEffect.value; break;
                }
            }
        }

        // 2. 计算全属性乘区（来自水牌）
        float allStatMult = 1 + TempAllStatBonus;

        // 3. 最终攻击力 = (基础攻击) * (1 + 火/土百分比) * 动作倍率 * 全属性乘区
        float finalAttack = BaseAttack * (1 + TempAtkPercentBonus) * actionCardDmgMult * allStatMult;

        // 4. 最终暴击率 = (基础暴击 + 金临时 + 动作暴击) * 全属性乘区
        float finalCritRate = Mathf.Min(1f, (BaseCritRate + TempCritRateBonus + actionCardCritRate) * allStatMult);
        // 最终暴伤 = (基础暴伤 + 动作暴伤) * 全属性乘区
        float finalCritDmg = (BaseCritDamage + actionCardCritDmg) * allStatMult;
        // 最终命中率 = (基础命中 + 土临时) * 全属性乘区
        float finalHitRate = Mathf.Min(1f, (BaseHitRate + TempHitRateBonus) * allStatMult);

        // 5. 获取敌人防御（假设目标有 Defense 属性，否则默认 5）
        int enemyDefense = 5;
        if (target is EnemyView enemy)
            enemyDefense = enemy.DefensePower;  // 你需要在 EnemyView 中添加 Defense 字段

        // 6. 命中判定
        bool hit = Random.value <= finalHitRate;
        if (!hit)
        {
            Debug.Log($"{actionCard.CardId} 未命中！");
            yield break;
        }

        // 7. 暴击判定
        bool isCrit = Random.value <= finalCritRate;
        // 基础伤害 = 攻击力 - 防御（下限 0）
        int baseDamage = Mathf.Max(0, (int)finalAttack - enemyDefense);
        int physicalDamage = isCrit ? (int)(baseDamage * finalCritDmg) : baseDamage;

        // 8. 灼烧真实伤害（无视防御）
        int burnDamage = TempBurnStacks * 2;
        int totalDamage = physicalDamage + burnDamage;

        // 9. 回血计算（基于总伤害百分比 + 额外固定回血）
        int healAmount = 0;
        if (totalDamage > 0)
        {
            float lostPercent = 1f - (HeroSystem.Instance.HeroView.CurrentHealth / (float)HeroSystem.Instance.HeroView.MaxHealth);
            float extraHeal = TempExtraHealFlat * lostPercent * HeroSystem.Instance.HeroView.MaxHealth;
            healAmount = (int)(HeroSystem.Instance.HeroView.CurrentHealth * TempHealPercentTotal + extraHeal);
            if (healAmount > 0)
            {
                HealHeroGA healHeroGA = new(healAmount);
                ActionSystem.Instance.AddReaction(healHeroGA);
            }
        }

        // 10. 产生伤害 GameAction
        if (totalDamage > 0)
        {
            DealDamageGA damageGA = new(totalDamage, new List<CombatantView> { target });
            ActionSystem.Instance.AddReaction(damageGA);
        }

        // 11. 如果灼烧层数 > 0，施加持续伤害效果（单独灼烧 DOT 需要 ApplyBurnGA）
        if (TempBurnStacks > 0 && target is EnemyView enemyTarget)
        {
            ApplyBurnGA burnGA = new(enemyTarget, TempBurnStacks);
            ActionSystem.Instance.AddReaction(burnGA);
        }

        // 12. 打出攻击牌后，清空本回合临时状态（属性牌不再保留）
        ResetTurnState();

        yield return null;
    }

    /// <summary>
    /// 防御动作：根据当前临时防御+基础防御格挡敌人攻击(不会立即产生伤害)
    /// </summary>
    public int CalculateDefenseValue(ActionCard action)
    {
        float actionDefBonus = 0f;
        foreach (var effect in action.Effects)
        {
            if (effect is ActionStatEffect statEffect && statEffect.statType == ActionStatType.DefenseBonus)
                actionDefBonus += statEffect.value;
        }

        float allStatMult = 1 + TempAllStatBonus;
        int finalDefense = (int)((BaseDefense + TempDefFlat + actionDefBonus) * allStatMult);

        // 防御后清空临时状态（属性牌加成消失）
        ResetTurnState();

        return finalDefense;
    }

    // ======================= 私有工具方法 =======================

    /// <summary>
    /// 根据元素和等级获取基础效果数值（硬编码表，可改为 ScriptableObject 配置）
    /// </summary>
    private CombatStats GetBaseEffect(ElementCard elementCard)
    {
        CombatStats combatStats=new CombatStats();
        foreach(var effect in elementCard.Effects)
        {
            effect.Setup(combatStats);
        }
        return combatStats;
    }

}

