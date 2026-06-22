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

}
