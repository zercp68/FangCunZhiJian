using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaUI : MonoBehaviour
{
    [Header("魔力UI配置")]
    [SerializeField] private GameObject FirstMana;    // 第一个固定魔力图标
    [SerializeField] private GameObject manaPrefab;   // 后续魔力图标预制体
    [SerializeField] private Transform manaParent;    // 所有魔力图标的父物体
    [SerializeField] private float secondOffset = 90f; // 第一个到第二个间距
    [SerializeField] private float nextOffset = 110f;  // 往后每个间距

    /// <summary>
    /// 根据当前魔力值更新UI
    /// </summary>
    public void UpdateMana(int currentMana)
    {
        Debug.Log("更新魔力UI，当前魔力：" + currentMana);
        ClearExtraMana();

        if (currentMana <= 0)
        {
            FirstMana.SetActive(false);
            return;
        }

        FirstMana.SetActive(true);
        float baseX = FirstMana.transform.localPosition.x;
        float currentX = baseX + secondOffset; // 第二个图标的X坐标

        // 魔力≥2，生成第二个
        if (currentMana >= 2)
        {
            GameObject newMana = Instantiate(manaPrefab, manaParent);
            newMana.transform.localPosition = new Vector3(currentX, 0, 0);
        }

        // 魔力≥3，循环生成剩下的
        if (currentMana >= 3)
        {
            int needCreateCount = currentMana - 2;
            for (int i = 0; i < needCreateCount; i++)
            {
                currentX += nextOffset; // 每次累加110
                GameObject newMana = Instantiate(manaPrefab, manaParent);
                newMana.transform.localPosition = new Vector3(currentX, 0, 0);
            }
        }
    }

    /// <summary>
    /// 清理动态生成的魔力图标（保留第一个固定图标）
    /// </summary>
    private void ClearExtraMana()
    {
        // 倒序删除，避免遍历下标错乱
        for (int i = manaParent.childCount - 1; i > 0; i--)
        {
            Destroy(manaParent.GetChild(i).gameObject);
        }
    }
}