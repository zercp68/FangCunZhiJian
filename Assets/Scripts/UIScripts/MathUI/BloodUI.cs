using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BloodUI : MonoBehaviour
{
    [SerializeField] private Text txtBlood;
    [SerializeField] private Image imgBlood;
    public void UpdateBlood(int currentHp,int MaxHp)
    {
        Debug.Log(currentHp);
        updateTxtBlood(currentHp);
        updateImgBlood(currentHp,MaxHp);
    }
    public void updateImgBlood(int currentHp, int maxHp)
    {
        // 限制血量不超出0~maxHp
        int hp = Mathf.Clamp(currentHp, 0, maxHp);
        Debug.Log(hp);
        // 换算成0~1的FillAmount
        float fillValue = (float)hp / maxHp;
        imgBlood.fillAmount = fillValue;
    }
    public void updateTxtBlood(int currentHp)
    {
        txtBlood.text = currentHp.ToString();
    }

}
