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

    [SerializeField] private List<Card> Ncards = new List<Card>();
    [SerializeField] private List<Card> Rcards = new List<Card>();
    [SerializeField] private List<Card> SRcards = new List<Card>();
    [SerializeField] private List<Card> SRRcards = new List<Card>();

    // ---------- 新增：商店配置 ----------
    [Header("商店配置")]
    [SerializeField] private int storeSize = 6;               // 商店每次刷新的卡牌数量
    [SerializeField][Range(0, 1)] private float NProb = 0.6f;  // 普通卡概率
    [SerializeField][Range(0, 1)] private float RProb = 0.25f;   // 一般卡概率
    [SerializeField][Range(0, 1)] private float SRProb = 0.10f;       // 稀有卡概率
    [SerializeField][Range(0, 1)] private float SRRProb = 0.05f;       // 传说卡概率

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
        CardManager.Instance.SplitCardsByRarity(Ncards,Rcards,SRcards,SRRcards);
    }


    // ---------- 新增：刷新商店（根据概率抽取） ----------
    /// <summary>
    /// 调用这个方法就进行刷新
    /// </summary>
    public void RefreshStore()
    {
        StoreCards.Clear();

        // 1. 校验各卡池是否有卡牌，空池直接警告
        bool hasN = Ncards.Count > 0;
        bool hasR = Rcards.Count > 0;
        bool hasSR = SRcards.Count > 0;
        bool hasSRR = SRRcards.Count > 0;
        if (!hasN && !hasR && !hasSR && !hasSRR)
        {
            Debug.LogError("所有商店卡池全部为空，无法刷新商店！");
            return;
        }

        // 2. 概率总和校验
        float totalProb = NProb + RProb + SRProb + SRRProb;
        if (Mathf.Approximately(totalProb, 0f))
        {
            Debug.LogError("商店四种卡牌概率总和为0，无法抽卡");
            return;
        }

        // 3. 归一化概率（防止玩家面板概率加起来不等于1）
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
        Debug.Log($"商店刷新完成，当前商店卡牌数量：{StoreCards.Count}");
    }

    /// <summary>
    /// 根据归一化随机roll稀有度，返回随机卡牌
    /// 抽卡区间：
    /// 0 ~ normN           = N普通
    /// normN ~ normN+normR = R精良
    /// normN+normR ~ sumSR = SR稀有
    /// sumSR ~ 1           = SRR传说
    /// </summary>
    private Card GetRandomCardByRarity(float normN, float normR, float normSR, float normSRR)
    {
        float roll = Random.Range(0f, 1f);
        float rThreshold = normN;
        float srThreshold = normN + normR;
        float srrThreshold = normN + normR + normSR;

        List<Card> targetPool = null;

        // 按roll值匹配稀有度卡池
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

        // 如果目标卡池为空，逐级降级兜底
        if (targetPool.Count == 0)
        {
            if (targetPool == SRRcards && SRcards.Count > 0) targetPool = SRcards;
            else if (targetPool == SRcards && Rcards.Count > 0) targetPool = Rcards;
            else if (targetPool == Rcards && Ncards.Count > 0) targetPool = Ncards;
            else if (targetPool == Ncards)
            {
                // N也空，兜底随便找一个有内容的池子
                if (Rcards.Count > 0) targetPool = Rcards;
                else if (SRcards.Count > 0) targetPool = SRcards;
                else if (SRRcards.Count > 0) targetPool = SRRcards;
            }
        }

        // 全部池子都空
        if (targetPool == null || targetPool.Count == 0)
        {
            Debug.LogWarning("抽卡时无可用卡池");
            return null;
        }

        // 从目标池随机返回一张
        int randomIdx = Random.Range(0, targetPool.Count);
        return targetPool[randomIdx];
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
        foreach (var card in cardsToBuy)
        {
            if (card == null)
            {
                Debug.LogWarning("待购买卡牌为空，跳过");
                continue;
            }


            if (card is WeaponCard weaponCard)
            {
                // weapon 和 card 是同一个实例，Weapon独有的字段不会丢失
                PlayerDataManager.Instance.AddCardToWeaponDeck(weaponCard);
                Debug.Log($"购入武器卡{weaponCard.CardId}，存入武器卡组");
            }
            else
            {
                // 普通卡牌，原始card完整传入
                PlayerDataManager.Instance.AddCardToDeck(card);
                Debug.Log($"购入普通卡{card.CardId}，存入主卡组");
            }
        }

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


}