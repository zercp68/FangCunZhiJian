using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 单条合成配方
[Serializable]
public class SynthesisRecipe
{
    [Header("合成所需材料卡牌 ID 列表")]
    public List<int> materialCardIds = new List<int>();
    [Header("合成产出卡牌 ID")]
    public int resultCardId;
}
public class SynthesisRecipeTable : Singleton<SynthesisRecipeTable>
{
    [Header("=== 所有合成配方列表 ===")]
    public List<SynthesisRecipe> allRecipes = new List<SynthesisRecipe>();

    /// <summary>
    /// 根据选中的卡牌ID列表，匹配对应配方（无序匹配）
    /// </summary>
    public SynthesisRecipe GetMatchRecipe(List<int> selectCardIds)
    {
        foreach (var recipe in allRecipes)
        {
            // 数量不一致直接跳过
            if (selectCardIds.Count != recipe.materialCardIds.Count)
                continue;

            // 比对两组ID是否完全一致（顺序无关）
            if (IsListEqual(selectCardIds, recipe.materialCardIds))
            {
                return recipe;
            }
        }
        return null;
    }

    // 两个int列表 无序对比
    private bool IsListEqual(List<int> a, List<int> b)
    {
        List<int> temp = new List<int>(b);
        foreach (int id in a)
        {
            if (!temp.Contains(id)) return false;
            temp.Remove(id);
        }
        return temp.Count == 0;
    }
}

