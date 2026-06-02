using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponCardData", menuName = "Data/Card/Action")]
public class ActionCardData : CardData
{
    /// <summary>
    /// 适应的武器类型
    /// </summary>
    [field: SerializeField] public List<E_WeaponType> E_validWeaponTypes { get; private set; }

    /// <summary>
    ///true = 防御动作，false = 攻击动作
    /// </summary>
    [field: SerializeField]public bool isDefenseAction { get; private set; }  
}
