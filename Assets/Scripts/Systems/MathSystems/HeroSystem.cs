using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSystem : Singleton<HeroSystem>
{
    [field:SerializeField]public HeroView HeroView { get;private set; }
    

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
        yield break;
    }
    public IEnumerator ApplyBurnPerformer(ApplyBurnGA applyBurnGA)
    {
        yield break;
    }
}
