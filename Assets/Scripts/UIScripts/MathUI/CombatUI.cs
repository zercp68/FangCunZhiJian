using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatUI : MonoBehaviour
{
    [Header("核心数值")]
    [SerializeField] private Text txtTotalBase;    // 总基础 = 基础攻击 + 部件加成
    [SerializeField] private Text txtTotalMult;    // 总倍率 = 元素倍率 × 动作倍率
    //[SerializeField] private Text txtFinalDamage;  // 最终伤害（可选，如要显示）

    [Header("防御")]
    [SerializeField] private Text txtTotalDef;     // 总防御 = 基础防御 + 防御加成

    /// <summary>
    /// 更新攻击核心显示
    /// </summary>
    public void UpdateCore(float totalBase, float totalMult)
    {
        txtTotalBase.text = Mathf.RoundToInt(totalBase).ToString();
        txtTotalMult.text = $"{totalMult:F2}";
    }

    /// <summary>
    /// 更新最终伤害
    /// </summary>
    //public void UpdateFinalDamage(float damage)
    //{
    //    if (txtFinalDamage != null)
    //        txtFinalDamage.text = Mathf.RoundToInt(damage).ToString();
    //}

    /// <summary>
    /// 更新防御显示
    /// </summary>
    public void UpdateDef(float totalDef)
    {
        if (txtTotalDef != null)
            txtTotalDef.text = Mathf.RoundToInt(totalDef).ToString();
    }

}
