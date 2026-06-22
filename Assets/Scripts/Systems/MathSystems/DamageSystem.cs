using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSystem : Singleton<DamageSystem>
{
    /// <summary>
    /// 特效Prefab
    /// </summary>
    [SerializeField] private GameObject heroDamageVFX;
    [SerializeField] private GameObject enemyDamageVFX;
    // 特效存活时长，统一控制
    [SerializeField] private float vfxDuration = 1f;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<DealDamageGA>(DealDamagePerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<DealDamageGA>();
    }

    private IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA)
    {
        foreach (var target in dealDamageGA.Targets)
        {
            if (target is EnemyView enemyView)
            {
                // 敌人受伤：使用敌人特效
                GameObject vfx = Instantiate(enemyDamageVFX, target.transform.position, Quaternion.identity);
                Destroy(vfx, vfxDuration); // 自动销毁
                target.Damage(dealDamageGA.Amount);
            }
            if (target is HeroView heroView)
            {
                Debug.Log($"目标类型：{target.GetType().Name}");
                // 英雄受伤：使用英雄特效
                GameObject vfx = Instantiate(heroDamageVFX, target.transform.position, Quaternion.identity);
                Destroy(vfx, vfxDuration); // 自动销毁
                target.Damage(dealDamageGA.Amount);
            }

            yield return new WaitForSeconds(0.15f);
            if (target.currentHp <= 0)
            {
                if (target is EnemyView deadEnemyView)
                {
                    KillEnemyGA killEnemyGA = new(deadEnemyView);
                    ActionSystem.Instance.AddReaction(killEnemyGA);
                    LevelManager.Instance.NextLevel();
                }
                else if(target is HeroView)
                {
                    UIManager.Instance.HidePanel<MathPanel>();
                    UIManager.Instance.ShowPanel<FailurePanel>();
                }
            }
        }
    }
}