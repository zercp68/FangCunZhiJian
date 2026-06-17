using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 战斗数值核心系统（MonoBehaviour单例）
/// 严格遵循表格数值体系，保留五行相生翻倍机制
/// </summary>
public class CombatSystem : MonoBehaviour
{
    // ======================= 配置常量 =======================
    private const float BASE_HIT_RATE = 0.8f;       // 固定命中率80%
    private const float DEFAULT_CRIT_DAMAGE = 2f; // 默认暴伤150%
    private const int BASE_ATTACK = 1;              // 表格基础攻击
    private const int BASE_DEFENSE = 0;              // 表格基础防御

    // ======================= 五行相生（触发翻倍） =======================
    // Key: 当前元素, Value: 前一个元素（满足则当前效果翻倍）
    private readonly Dictionary<E_ElementType, E_ElementType> generateMap = new()
    {
        { E_ElementType.Water, E_ElementType.Metal },  // 金 → 水
        { E_ElementType.Wood,  E_ElementType.Water },  // 水 → 木
        { E_ElementType.Fire,  E_ElementType.Wood  },  // 木 → 火
        { E_ElementType.Earth, E_ElementType.Fire  },  // 火 → 土
        { E_ElementType.Metal, E_ElementType.Earth },  // 土 → 金
    };

    // ======================= 运行时状态 =======================
    public WeaponCard CurrentWeapon { get; private set; }
    private List<ElementCard> tempElementCards = new();   // 本回合打出的属性牌顺序

    // 武器常驻数值（武器加成 + 基础值）
    public float BaseAttack { get; private set; } = BASE_ATTACK;
    public float BaseDefense { get; private set; } = BASE_DEFENSE;
    public float BaseCritRate { get; private set; } = 0f;
    public float BaseCritDamage { get; private set; } = DEFAULT_CRIT_DAMAGE;
    public float BaseHitRate { get; private set; } = BASE_HIT_RATE;
    public float ElementMult { get; private set; } = 1f;   // 剑的额外增幅

    // 临时加成（单回合，由属性牌累积）
    public float TempAllStatBonus { get; private set; }      // 水：全属性加成
    public float TempAtkPercentBonus { get; private set; }   // 火：伤害百分比加成
    public int TempDefFlat { get; private set; }             // 土：固定防御加成
    public float TempCritRateBonus { get; private set; }     // 金：暴击率加成
    public int TempBurnStacks { get; private set; }          // 火：灼烧层数（本次攻击附加真实伤害 = 层数×2）
    public float TempHealPercentTotal { get; private set; }  // 木：基于伤害的回血比例总和
    public int TempExtraHealFlat { get; private set; }       // 四木：额外固定回血

    // ======================= 公共方法 =======================
    /// <summary> 重置回合临时状态（回合开始时调用）</summary>
    public void ResetTurnState()
    {
        tempElementCards.Clear();
        TempAllStatBonus = 0f;
        TempAtkPercentBonus = 0f;
        TempDefFlat = 0;
        TempCritRateBonus = 0f;
        TempBurnStacks = 0;
        TempHealPercentTotal = 0f;
        TempExtraHealFlat = 0;
    }

    /// <summary> 装备武器（从武器卡读取数值）</summary>
    public void SetupWeapon(WeaponCard weapon)
    {
        CurrentWeapon = weapon;
        if (CurrentWeapon == null)
        {
            Debug.LogError("装备武器失败：武器卡为空");
            return;
        }
        BaseAttack = BASE_ATTACK + weapon.attackBonus;
        BaseDefense = BASE_DEFENSE + weapon.defenseBonus;
        BaseCritRate = weapon.baseCriRate;
        BaseCritDamage = weapon.baseCritDamage != 0 ? weapon.baseCritDamage : DEFAULT_CRIT_DAMAGE;
        BaseHitRate = BASE_HIT_RATE;
        ElementMult = weapon.elementMult;
    }

    /// <summary> 打出一张属性牌，累加临时效果（支持相生翻倍）</summary>
    public void AddElementCard(ElementCard elementCard)
    {
        Debug.Log(elementCard.E_ElementType);
        ElementCard previous = tempElementCards.Count > 0 ? tempElementCards.Last() : null;
        tempElementCards.Add(elementCard);

        // 获取该属性牌的效果数值（从Effect列表中读取）
        CombatStats stats = GetBaseEffect(elementCard);
        bool trigger = previous != null && generateMap.ContainsKey(elementCard.E_ElementType)
                                      && generateMap[elementCard.E_ElementType] == previous.E_ElementType;

        switch (elementCard.E_ElementType)
        {
            case E_ElementType.Metal:
                float finalCrit = stats.critRateBonus;
                if (trigger) finalCrit *= 2;
                TempCritRateBonus += finalCrit;
                break;
            case E_ElementType.Wood:
                float finalHeal = stats.healPercent;
                if (trigger) finalHeal *= 2;
                TempHealPercentTotal += finalHeal;
                TempExtraHealFlat += stats.extraHealFromLost;
                break;
            case E_ElementType.Water:
                float finalAllStat = stats.allStatBonus;
                if (trigger) finalAllStat *= 2;
                TempAllStatBonus += finalAllStat;
                break;
            case E_ElementType.Fire:
                float finalDmg = stats.damagePercentBonus;
                if (trigger) finalDmg *= 2;
                TempAtkPercentBonus += finalDmg;
                int finalBurn = stats.burnStacks;
                if (trigger) finalBurn *= 2;
                TempBurnStacks += finalBurn;
                break;
            case E_ElementType.Earth:
                int finalDef = stats.defenseFlatBonus;
                if (trigger) finalDef *= 2;
                TempDefFlat += finalDef;
                break;
        }
    }

    /// <summary> 执行攻击动作，计算伤害并产生原子动作 </summary>
    public IEnumerator ExecuteAttack(ActionCard actionCard, EnemyView target)
    {
        // 1. 读取动作卡加成（暴击率、暴伤、伤害倍率）
        float actionCritRate = 0f, actionCritDmg = 0f, actionDmgMult = 1f;
        foreach (var effect in actionCard.Effects)
        {
            if (effect is ActionStatEffect statEffect)
            {
                switch (statEffect.statType)
                {
                    case ActionStatType.CritRateBonus: actionCritRate += statEffect.value; break;
                    case ActionStatType.CritDamageBonus: actionCritDmg += statEffect.value; break;
                    case ActionStatType.DamageMultiplier: actionDmgMult *= statEffect.value; break;
                }
            }
        }

        // 2. 全属性乘区（来自水牌）
        float allStatMult = 1 + TempAllStatBonus;

        // 3. 最终攻击力 = 基础攻击 × (1+临时攻击加成) × 动作倍率 × 全属性乘区
        float finalAttack = BaseAttack * (1 + TempAtkPercentBonus) * actionDmgMult * allStatMult;

        // 4. 最终暴击率 / 暴伤（命中率固定0.8，不参与计算）
        float finalCritRate = Mathf.Min(1f, (BaseCritRate + TempCritRateBonus + actionCritRate) * allStatMult);
        float finalCritDmg = (BaseCritDamage + actionCritDmg) * allStatMult;
        

        finalCritDmg = 2;
        Debug.Log(BaseCritRate);
        Debug.Log(TempCritRateBonus);
        Debug.Log(actionCritRate);
        Debug.Log(allStatMult);
        Debug.Log(finalCritRate);
        Debug.Log(finalCritDmg);


        // 5. 命中判定（固定80%）
        bool hit = Random.value <= BASE_HIT_RATE;
        if (!hit)
        {
            Debug.Log($"{actionCard.CardId} 未命中！");
            yield break;
        }

        // 6. 暴击判定 & 基础伤害计算
        bool isCrit = Random.value <= finalCritRate;
        int baseDamage = Mathf.Max(0, (int)finalAttack - target.DefensePower);
        int physicalDamage = isCrit ? (int)(baseDamage * finalCritDmg) : baseDamage;
        Debug.Log(physicalDamage);

        // 7. 灼烧真实伤害（每层2点，本次攻击附加）
        int burnDamage = TempBurnStacks * 2;
        int totalDamage = physicalDamage + burnDamage;

        // 8. 回血计算（基于总伤害比例 + 额外固定回血）
        int healAmount = 0;
        if (totalDamage > 0)
        {
            healAmount = (int)(totalDamage * TempHealPercentTotal) + TempExtraHealFlat;
            if (healAmount > 0)
            {
                ActionSystem.Instance.AddReaction(new HealHeroGA(healAmount));
            }
        }
        Debug.Log(totalDamage);
        // 9. 造成伤害
        if (totalDamage > 0)
        {
            ActionSystem.Instance.AddReaction(new DealDamageGA(totalDamage, new List<CombatantView> { target }));
        }

        // 10. 施加灼烧持续效果（后续每回合造成伤害）
        if (TempBurnStacks > 0)
        {
            ActionSystem.Instance.AddReaction(new ApplyBurnGA(target, TempBurnStacks));
        }

        // 11. 清空本回合临时状态（属性牌仅本次攻击生效）
        ResetTurnState();

        yield return null;
    }

    /// <summary> 计算防御动作提供的防御值（用于格挡敌人攻击）</summary>
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

        // 防御后清空临时状态
        ResetTurnState();
        return finalDefense;
    }

    // ======================= 私有辅助 =======================
    private CombatStats GetBaseEffect(ElementCard elementCard)
    {
        Debug.Log(elementCard.Effects[0]);

        CombatStats stats = new CombatStats();
        foreach (var effect in elementCard.Effects)
        {
            effect.Setup(stats);
        }
        return stats;
    }
}

