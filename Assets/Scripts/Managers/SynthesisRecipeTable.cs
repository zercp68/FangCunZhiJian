using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 合成配方表（使用 CardCraftRecipe ScriptableObject）
/// </summary>
public class SynthesisRecipeTable : Singleton<SynthesisRecipeTable>
{
    [Header("=== 所有合成配方列表 ===")]
    public List<CardCraftRecipe> allRecipes = new List<CardCraftRecipe>();

    /// <summary>
    /// 根据选中的卡牌列表，匹配对应配方（无序匹配）
    /// </summary>
    /// <param name="materialCards">材料卡牌列表</param>
    /// <returns>匹配的配方，没有则返回 null</returns>
    public CardCraftRecipe GetMatchRecipe(List<Card> materialCards)
    {
        if (materialCards == null || materialCards.Count == 0)
            return null;

        // 提取材料卡牌的 ID 列表
        List<int> materialIds = materialCards.Select(c => c.CardId).ToList();

        foreach (var recipe in allRecipes)
        {
            if (recipe == null) continue;

            // 提取配方中材料卡牌的 ID 列表
            List<int> recipeIds = recipe.inputCards.Select(c => c.cardId).ToList();

            // 数量不一致直接跳过
            if (materialIds.Count != recipeIds.Count)
                continue;

            // 无序比对两组 ID 是否完全一致
            if (IsListEqual(materialIds, recipeIds))
            {
                return recipe;
            }
        }
        return null;
    }

    /// <summary>
    /// 根据选中的卡牌数据列表，匹配对应配方（用于 ScriptableObject 引用）
    /// </summary>
    public CardCraftRecipe GetMatchRecipeByData(List<CardData> materialCardData)
    {
        if (materialCardData == null || materialCardData.Count == 0)
            return null;

        List<int> materialIds = materialCardData.Select(c => c.cardId).ToList();

        foreach (var recipe in allRecipes)
        {
            if (recipe == null) continue;

            List<int> recipeIds = recipe.inputCards.Select(c => c.cardId).ToList();

            if (materialIds.Count != recipeIds.Count)
                continue;

            if (IsListEqual(materialIds, recipeIds))
            {
                return recipe;
            }
        }
        return null;
    }

    /// <summary>
    /// 无序比对两个 int 列表是否相等
    /// </summary>
    private bool IsListEqual(List<int> a, List<int> b)
    {
        if (a.Count != b.Count) return false;

        List<int> temp = new List<int>(b);
        foreach (int id in a)
        {
            if (!temp.Contains(id)) return false;
            temp.Remove(id);
        }
        return temp.Count == 0;
    }
}