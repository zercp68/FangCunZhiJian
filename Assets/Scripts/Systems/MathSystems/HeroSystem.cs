using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSystem : Singleton<HeroSystem>
{
    [SerializeField] private GameObject HealHeroVFX;
    // 特效存活时长，统一控制
    [SerializeField] private float vfxDuration = 1f;
    [field:SerializeField]public HeroView HeroView { get;private set; }
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AttackEnemyGA>(AttackEnemyPerformer);
        ActionSystem.AttachPerformer<HealHeroGA>(HealHeroPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AttackEnemyGA>();
        ActionSystem.DetachPerformer<HealHeroGA>();
    }

    public void Setup(HeroData heroData)
    {
        HeroView.Setup(heroData);
    }
    public IEnumerator AttackEnemyPerformer(AttackEnemyGA attackEnemyGA)
    {
        yield break;
    }
    public IEnumerator HealHeroPerformer(HealHeroGA healHeroGA)
    {
        GameObject vfx = Instantiate(HealHeroVFX, transform.position, Quaternion.identity);
        Destroy(vfx, vfxDuration); 

        HeroView.Heal(healHeroGA.Amount);
        yield break;
    }
    // 在 HeroView 或 HeroSystem 中处理英雄受伤前
    public int ApplyDefenseBeforeDamage(int incomingDamage)
    {
        // 1. 免疫检测
        if (CombatSystem.Instance.TempImmuneCount > 0)
        {
            CombatSystem.Instance.TempImmuneCount--;
            return 0; // 完全免疫
        }

        // 2. 伤害减免
        float reduction = CombatSystem.Instance.TempDamageReduction;
        incomingDamage = Mathf.RoundToInt(incomingDamage * (1 - reduction));

        // 3. 反弹伤害（这里只是收集数据，真正反弹在别处处理）
        if (CombatSystem.Instance.TempReflectPercent > 0)
        {
            int reflectDamage = Mathf.RoundToInt(incomingDamage * CombatSystem.Instance.TempReflectPercent);
            // 生成 DealDamageGA 反弹给攻击者（需要知道攻击者是谁）
        }

        return Mathf.Max(0, incomingDamage);
    }
}
