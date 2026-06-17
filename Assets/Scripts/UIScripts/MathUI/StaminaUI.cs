using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaUI : MonoBehaviour
{
    [SerializeField] private GameObject Stamina1;
    [SerializeField] private GameObject Stamina2;
    [SerializeField] private GameObject Stamina3;

    public void UpdateStamina(int stamina)
    {
        HideAllStamina();

        // 限制区间 ，超出不显示
        if (stamina <= 0)
            return;

        if (stamina >= 1) Stamina1.SetActive(true);
        if (stamina >= 2) Stamina2.SetActive(true);
        if (stamina >= 3) Stamina3.SetActive(true);
    }

    /// <summary>
    /// 隐藏全部耐力图标
    /// </summary>
    private void HideAllStamina()
    {
        Stamina1.SetActive(false);
        Stamina2.SetActive(false);
        Stamina3.SetActive(false);
    }
}