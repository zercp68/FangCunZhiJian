using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StatusEffectsUI : MonoBehaviour
{
    [SerializeField] private StatusEffectUI statusEffectUIPrefab;
    [SerializeField] private Sprite armorSprite, burnSprite;
    private Dictionary<StatusEffectType, StatusEffectUI> statusEffectUIs = new();

    public void UpdateStatusEffectUI(StatusEffectType statusEffectType,int stackCount)
    {
        if (stackCount == 0) 
        {
            if (statusEffectUIs.ContainsKey(statusEffectType)) 
            {
                StatusEffectUI statusEffectUI = statusEffectUIs[statusEffectType];
                statusEffectUIs.Remove(statusEffectType);
                Destroy(statusEffectUI);
            }
        }
        else
        {
            if (!statusEffectUIs.ContainsKey(statusEffectType))
            {
                StatusEffectUI statusEffectUI = Instantiate(statusEffectUIPrefab, transform);
                statusEffectUIs.Add(statusEffectType, statusEffectUI);
            }
            Sprite sprite=GetSpriteByType(statusEffectType);
            statusEffectUIs[statusEffectType].Set(sprite,stackCount);
        }
    }

    /// <summary>
    /// 获取对应的精灵
    /// </summary>
    /// <param name="statusEffectType"></param>
    /// <returns></returns>
    private Sprite GetSpriteByType(StatusEffectType statusEffectType)
    {
        return statusEffectType switch { 
            StatusEffectType.ARMOR=>armorSprite,
            StatusEffectType.BURN=>burnSprite,
            _=>null};
    } 
}
