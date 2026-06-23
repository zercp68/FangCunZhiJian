using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreDataManager : Singleton<StoreDataManager>
{
    private List<Card> currentStoreCards = new List<Card>();
    private List<Card> Ncards = new List<Card>();
    private List<Card> Rcards = new List<Card>();
    private List<Card> SRcards = new List<Card>();
    private List<Card> SRRcards = new List<Card>();
    public void setup()
    {
        // 先校验 CardManager 实例是否存在
        if (CardManager.Instance == null)
        {
            Debug.LogError("CardManager 实例不存在！");
            return;
        }
        // 调用拆分方法前，确保列表已实例化（避免传入 null）
        if (Ncards == null) Ncards = new List<Card>();
        if (Rcards == null) Rcards = new List<Card>();
        if (SRcards == null) SRcards = new List<Card>();
        if (SRRcards == null) SRRcards = new List<Card>();

        CardManager.Instance.SplitCardsByRarity(Ncards, Rcards, SRcards, SRRcards);

        // 校验拆分结果，提前预警
        if (Ncards.Count == 0 && Rcards.Count == 0 && SRcards.Count == 0 && SRRcards.Count == 0)
        {
            Debug.LogError("拆分稀有度后所有卡片列表为空！");
        }
    }
    /// 返回商店当前刷新出的卡牌副本
    public List<Card> GetcurrentStoreCards()
    {
        return new List<Card>(currentStoreCards);
    }
    // 给外部修改商店当前卡牌的方法
    public void SetStoreCards(List<Card> newStoreCards)
    {
        currentStoreCards.Clear();
        if (newStoreCards != null)
            currentStoreCards.AddRange(newStoreCards);
        Debug.Log(currentStoreCards.Count);

    }
    /// <summary>
    /// 获取拆分好的四种稀有度商店卡池
    /// </summary>
    /// <param name="outN">普通卡输出列表</param>
    /// <param name="outR">精良卡输出列表</param>
    /// <param name="outSR">稀有卡输出列表</param>
    /// <param name="outSRR">传说卡输出列表</param>
    public void GetRaritysCards(out List<Card> outN, out List<Card> outR, out List<Card> outSR, out List<Card> outSRR)
    {
        // 返回副本，防止外部篡改内部原始列表
        outN = new List<Card>(Ncards);
        outR = new List<Card>(Rcards);
        outSR = new List<Card>(SRcards);
        outSRR = new List<Card>(SRRcards);
    }
}
