using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoreSystem : Singleton<StoreSystem>
{
    [SerializeField] private StoreCardHolder storeCardHolder1;
    private readonly List<Card> StoreCards = new List<Card>();

    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;

    private List<Card> Ncards = new List<Card>();
    private List<Card> Rcards = new List<Card>();
    private List<Card> SRcards = new List<Card>();
    private List<Card> SRRcards = new List<Card>();

    // ---------- 商店概率配置 ----------
    [Header("概率配置")]
    [SerializeField] private int storeSize = 6;               // 商店每次刷新的卡牌数量
    [SerializeField][Range(0, 1)] private float NProb = 0.6f;  // 普通卡概率
    [SerializeField][Range(0, 1)] private float RProb = 0.25f;   // 稀有卡概率
    [SerializeField][Range(0, 1)] private float SRProb = 0.10f;       // 超稀有卡概率
    [SerializeField][Range(0, 1)] private float SRRProb = 0.05f;       // 史诗卡概率

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<RethrowGA>(RethrowPerformer);
        ActionSystem.AttachPerformer<DrawStoreCardsGA>(DrawStoreCardsPerformer);
        ActionSystem.AttachPerformer<BuyCardGA>(BuyCardPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<RethrowGA>();
        ActionSystem.DetachPerformer<DrawStoreCardsGA>();
        ActionSystem.DetachPerformer<BuyCardGA>();
    }

    public void setUp()
    {
        // 校验 CardManager 实例是否存在
        if (StoreDataManager.Instance == null)
        {
            Debug.LogError("CardManager 实例不存在！");
            return;
        }
        // 初始化卡牌列表（防止空引用）
        if (Ncards == null) Ncards = new List<Card>();
        if (Rcards == null) Rcards = new List<Card>();
        if (SRcards == null) SRcards = new List<Card>();
        if (SRRcards == null) SRRcards = new List<Card>();

        StoreDataManager.Instance.GetRaritysCards(out Ncards, out Rcards, out SRcards, out SRRcards);

        // 校验卡牌列表初始化前预警
        if (Ncards.Count == 0 && Rcards.Count == 0 && SRcards.Count == 0 && SRRcards.Count == 0)
        {
            Debug.LogError("按稀有度分类的卡牌列表为空！");
        }
    }

    // ---------- 核心修改：刷新商店逻辑 ----------
    /// <summary>
    /// 根据稀有度概率刷新商店
    /// 优先使用 StoreDataManager 中缓存的卡牌，无缓存时才重新抽取
    /// </summary>
    public void RefreshStore()
    {
        StoreCards.Clear();

        // 1. 先从 StoreDataManager 获取缓存的商店卡牌
        List<Card> cachedCards = StoreDataManager.Instance.GetcurrentStoreCards();
        if (cachedCards != null && cachedCards.Count > 0)
        {
            // 有缓存：直接使用缓存卡牌（保证数量匹配 storeSize）
            StoreCards.AddRange(cachedCards.Take(storeSize));
            Debug.Log($"使用缓存卡牌，数量：{StoreCards.Count}");
            return;
        }

        // 2. 无缓存：执行原有的随机抽取逻辑
        // 校验卡牌池是否有卡牌
        bool hasN = Ncards.Count > 0;
        bool hasR = Rcards.Count > 0;
        bool hasSR = SRcards.Count > 0;
        bool hasSRR = SRRcards.Count > 0;
        if (!hasN && !hasR && !hasSR && !hasSRR)
        {
            Debug.LogError("商店卡牌池全部为空，无法刷新商店！");
            return;
        }

        // 概率总和校验
        float totalProb = NProb + RProb + SRProb + SRRProb;
        if (Mathf.Approximately(totalProb, 0f))
        {
            Debug.LogError("商店卡牌概率总和为0，无法抽取");
            return;
        }

        // 概率归一化（防止总和不为1）
        float normN = NProb / totalProb;
        float normR = RProb / totalProb;
        float normSR = SRProb / totalProb;
        float normSRR = SRRProb / totalProb;

        // 循环抽取对应数量卡牌
        for (int i = 0; i < storeSize; i++)
        {
            Card selectedCard = GetRandomCardByRarity(normN, normR, normSR, normSRR);
            if (selectedCard != null)
                StoreCards.Add(selectedCard);
        }

        // 3. 将抽取的卡牌存入 StoreDataManager 缓存（关键：保留本次抽取结果）
        StoreDataManager.Instance.SetStoreCards(StoreCards);
        Debug.Log(StoreCards.Count);
        Debug.Log($"商店刷新完成（新抽取），当前商店卡牌数量：{StoreCards.Count}");
    }

    /// <summary>
    /// 根据归一化概率roll稀有度，随机获取卡牌
    /// </summary>
    private Card GetRandomCardByRarity(float normN, float normR, float normSR, float normSRR)
    {
        float roll = Random.Range(0f, 1f);
        float rThreshold = normN;
        float srThreshold = normN + normR;
        float srrThreshold = normN + normR + normSR;

        List<Card> targetPool = null;

        // 按roll值匹配稀有度池
        if (roll < rThreshold)
        {
            targetPool = Ncards;
        }
        else if (roll < srThreshold)
        {
            targetPool = Rcards;
        }
        else if (roll < srrThreshold)
        {
            targetPool = SRcards;
        }
        else
        {
            targetPool = SRRcards;
        }

        // 目标池为空时的降级逻辑
        if (targetPool.Count == 0)
        {
            if (targetPool == SRRcards && SRcards.Count > 0) targetPool = SRcards;
            else if (targetPool == SRcards && Rcards.Count > 0) targetPool = Rcards;
            else if (targetPool == Rcards && Ncards.Count > 0) targetPool = Ncards;
            else if (targetPool == Ncards)
            {
                // N也空，反向找第一个有卡牌的池
                if (Rcards.Count > 0) targetPool = Rcards;
                else if (SRcards.Count > 0) targetPool = SRcards;
                else if (SRRcards.Count > 0) targetPool = SRRcards;
            }
        }

        // 全部池都空
        if (targetPool == null || targetPool.Count == 0)
        {
            Debug.LogWarning("抽取时无可用卡牌");
            return null;
        }

        // 从目标池随机选一张
        int randomIdx = Random.Range(0, targetPool.Count);
        return targetPool[randomIdx];
    }

    // 随机抽取卡牌的辅助方法（保留）
    private Card GetRandomCardFromList(List<Card> cardList)
    {
        int index = Random.Range(0, cardList.Count);
        Card original = cardList[index];
        return original;
    }

    /// <summary>
    /// 抽取商店卡牌的执行逻辑
    /// </summary>
    private IEnumerator DrawStoreCardsPerformer(DrawStoreCardsGA drawStoreCardsGA)
    {
        int needDraw = storeSize;
        for (int i = 0; i < needDraw; i++)
        {
            if (StoreCards.Count == 0)
            {
                RefreshStore();
                // 刷新后还是空，直接退出
                if (StoreCards.Count == 0)
                    yield break;
            }
        }

        // 添加到卡牌 Holder 并执行UI动画
        foreach (var card in StoreCards)
        {
            yield return storeCardHolder1.AddCard(card, drawPilePoint);
        }
        yield break;
    }

    private IEnumerator RethrowPerformer(RethrowGA rethrowGA)
    {
        // 重掷时清空缓存（保证重掷后是新卡牌）
        StoreDataManager.Instance.SetStoreCards(null);

        // 原重掷逻辑
        List<CardLogic> rethrowCardsCopy1 = new List<CardLogic>(storeCardHolder1.cardLogics);
        foreach (var cardLogic in rethrowCardsCopy1)
        {
            if (cardLogic != null)
            {
                yield return DiscardCard(cardLogic);
            }
        }
        yield return DrawStoreCardsPerformer(new DrawStoreCardsGA());
    }

    private IEnumerator BuyCardPerformer(BuyCardGA buyCardGA)
    {
        // 获取选中的卡牌逻辑
        List<CardLogic> selectedLogics = storeCardHolder1.GetAllSelectedCards();
        if (selectedLogics.Count == 0)
        {
            Debug.Log("没有选择任何卡牌，无法购买");
            yield break;
        }

        // 计算总花费
        int totalCost = 0;
        List<Card> cardsToBuy = new List<Card>();
        foreach (var cardLogic in selectedLogics)
        {
            if (cardLogic != null && cardLogic.card != null)
            {
                totalCost += cardLogic.card.CardMoney;   // 假设 Card 有 Price 属性
                cardsToBuy.Add(cardLogic.card);
            }
        }

        // 校验金币是否足够
        if (!PlayerDataManager.Instance.SpendMoney(totalCost))
        {
            Debug.Log($"金币不足，需要 {totalCost} 当前 {PlayerDataManager.Instance.Money}");
            yield break;
        }

        // 购买成功：移除商店卡牌并更新缓存
        foreach (var cardLogic in selectedLogics)
        {
            if (cardLogic != null)
            {
                yield return DiscardCard(cardLogic);
            }
        }

        // 更新 StoreDataManager 缓存（移除已购买的卡牌）
        StoreDataManager.Instance.SetStoreCards(StoreCards);

        // 将购买的卡牌加入玩家卡组
        foreach (var card in cardsToBuy)
        {
            if (card == null)
            {
                Debug.LogWarning("购买的卡牌为空，跳过");
                continue;
            }
            PlayerDataManager.Instance.AddCardToDeck(card);
        }
    }

 

    /// <summary>
    /// 弃置一张卡牌（移动到弃牌堆并销毁）
    /// </summary>
    private IEnumerator DiscardCard(CardLogic cardLogic)
    {
        if (cardLogic == null) yield break;

        // 从商店列表移除
        StoreCards.Remove(cardLogic.card);
        storeCardHolder1.RemoveCard(cardLogic.card);

        // 弃牌动画
        if (cardLogic.cardVisual != null)
        {
            Transform visualTransform = cardLogic.cardVisual.transform;
            Vector3 targetWorldPos = discardPilePoint.position;

            DG.Tweening.Sequence seq = DOTween.Sequence();
            seq.Append(visualTransform.DOMove(targetWorldPos, 0.2f).SetEase(Ease.InBack));
            seq.Join(visualTransform.DOScale(0, 0.2f).SetEase(Ease.InBack));
            yield return seq.WaitForCompletion();

            // 销毁视觉对象
            Destroy(cardLogic.cardVisual.gameObject);
        }

        // 销毁卡槽和逻辑对象
        if (cardLogic.slotGameObject != null)
        {
            Destroy(cardLogic.slotGameObject);
        }
        Destroy(cardLogic.gameObject);
    }
}