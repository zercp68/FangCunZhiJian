using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreSystem : Singleton<StoreSystem>
{


    [SerializeField] private StoreCardHolder storeCardHolder1;
    private readonly List<Card> StoreCards = new List<Card>();

    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;

    [SerializeField] private List<Card> ordinaryCards = new List<Card>();
    [SerializeField] private List<Card> generalCards = new List<Card>();
    [SerializeField] private List<Card> rareCards = new List<Card>();

    // ---------- 新增：商店配置 ----------
    [Header("商店配置")]
    [SerializeField] private int storeSize = 6;               // 商店每次刷新的卡牌数量
    [SerializeField][Range(0, 1)] private float ordinaryProb = 0.5f;  // 普通卡概率
    [SerializeField][Range(0, 1)] private float generalProb = 0.3f;   // 一般卡概率
    [SerializeField][Range(0, 1)] private float rareProb = 0.2f;       // 稀有卡概率

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



    public void setUp(List<CardData> ordinaryCardDatas, List<CardData> generalCardDatas, List<CardData> rareCardDatas)
    {
        CardManager.Instance.fillCardList(ordinaryCardDatas, ordinaryCards);
        CardManager.Instance.fillCardList(generalCardDatas, generalCards);
        CardManager.Instance.fillCardList(rareCardDatas, rareCards);
    }


    // ---------- 新增：刷新商店（根据概率抽取） ----------
    /// <summary>
    /// 调用这个方法就进行刷新
    /// </summary>
    public void RefreshStore()
    {
        StoreCards.Clear();

        // 校验概率总和（建议在 Inspector 中保证总和为1，这里做防御）
        float total = ordinaryProb + generalProb + rareProb;
        if (Mathf.Approximately(total, 0f))
        {
            Debug.LogError("商店概率总和为0，无法抽卡");
            return;
        }

        for (int i = 0; i < storeSize; i++)
        {
            Card selectedCard = GetRandomCardByRarity();
            if (selectedCard != null)
                StoreCards.Add(selectedCard);
        }
    }

    // 根据概率决定稀有度，然后从对应池子随机取一张卡
    private Card GetRandomCardByRarity()
    {
        float roll = Random.Range(0f, 1f);
        float ordinaryRange = ordinaryProb;
        float generalRange = ordinaryProb + generalProb;

        if (roll < ordinaryRange && ordinaryCards.Count > 0)
            return GetRandomCardFromList(ordinaryCards);
        else if (roll < generalRange && generalCards.Count > 0)
            return GetRandomCardFromList(generalCards);
        else if (rareCards.Count > 0)
            return GetRandomCardFromList(rareCards);
        else
        {
            // 降级处理：如果目标稀有度池为空，尝试其他池子
            if (ordinaryCards.Count > 0) return GetRandomCardFromList(ordinaryCards);
            if (generalCards.Count > 0) return GetRandomCardFromList(generalCards);
            if (rareCards.Count > 0) return GetRandomCardFromList(rareCards);
            Debug.LogWarning("所有卡池均为空，无法抽卡");
            return null;
        }
    }
    // 从列表中随机取一张卡（深拷贝可选，根据你的需求决定）
    private Card GetRandomCardFromList(List<Card> cardList)
    {
        int index = Random.Range(0, cardList.Count);
        Card original = cardList[index];
        return original;
    }

    /// <summary>
    /// 抽牌行为执行器
    /// </summary>
    /// <param name="drawCardsGA"></param>
    /// <returns></returns>
    private IEnumerator DrawStoreCardsPerformer(DrawStoreCardsGA drawStoreCardsGA)
    {
        int needDraw = storeSize;
        for (int i = 0; i < needDraw; i++)
        {
            if (StoreCards.Count == 0)
            {
                RefreshStore();
                //如果洗牌后还是空的，直接退出
                if (StoreCards.Count == 0)
                    yield break;
            }
            //执行单张抽牌
            yield return DrawCard();
        }
    }

    private IEnumerator RethrowPerformer(RethrowGA rethrowGA)
    {
        // 创建副本，避免在遍历时修改原集合
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
            Debug.Log("没有选中任何卡牌，无法购买");
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

        // 检查金币是否足够
        if (!PlayerDataManager.Instance.SpendMoney(totalCost))
        {
            Debug.Log($"金币不足，需要 {totalCost}，当前 {PlayerDataManager.Instance.Money}");
            yield break;
        }

        // 金币足够，执行购买：移除商店卡牌，添加到玩家卡组
        // 先逐一播放弃牌动画并销毁（因为 DiscardCard 是协程，需要顺序执行）
        foreach (var cardLogic in selectedLogics)
        {
            if (cardLogic != null)
            {  
                yield return DiscardCard(cardLogic);
            }
        }

        // 将购买的卡牌添加到玩家卡组
        PlayerDataManager.Instance.AddCardsToDeck(cardsToBuy);

        Debug.Log($"购买成功，共 {cardsToBuy.Count} 张卡牌，花费 {totalCost} 金币");
        
    }



    /// <summary>
    /// 抽取单张牌
    /// </summary>
    /// <returns></returns>
    private IEnumerator DrawCard()
    {
        //从牌堆中随机抽一张牌并移除（拓展方法）
        Card card = StoreCards.Draw();
        // 调用Holder.addCard方法就行        //创建卡牌UI(从牌堆位置生成)
        yield return storeCardHolder1.AddCard(card, drawPilePoint);


    }
    /// <summary>
    /// 弃掉一张卡牌（播放移动到弃牌堆的动画，然后销毁）
    /// </summary>
    private IEnumerator DiscardCard(CardLogic cardLogic)
    {
        if (cardLogic == null) yield break;

        // 【第一步】先从手牌系统移除（必须最先做！）
        StoreCards.Remove(cardLogic.card);

        storeCardHolder1.RemoveCard(cardLogic.card);


        // 【第二步】播放动画
        if (cardLogic.cardVisual != null)
        {
            Transform visualTransform = cardLogic.cardVisual.transform;
            Vector3 targetWorldPos = discardPilePoint.position;

            DG.Tweening.Sequence seq = DOTween.Sequence();
            seq.Append(visualTransform.DOMove(targetWorldPos, 0.2f).SetEase(Ease.InBack));
            seq.Join(visualTransform.DOScale(0, 0.2f).SetEase(Ease.InBack));
            yield return seq.WaitForCompletion();

            // 动画完销毁视觉
            Destroy(cardLogic.cardVisual.gameObject);
        }

        // 【第三步】销毁槽位 + 卡牌本体
        if (cardLogic.slotGameObject != null)
        {
            Destroy(cardLogic.slotGameObject);
        }

        Destroy(cardLogic.gameObject);
    }

    void Start()
    {

    }


    void Update()
    {

    }
}