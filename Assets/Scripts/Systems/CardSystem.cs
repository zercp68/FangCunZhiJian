using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SubsystemsImplementation;

public class CardSystem : Singleton<CardSystem>
{
    [SerializeField] private int HandAmount = 6;

    [SerializeField] private HorizontalCardHolder handHolder;
    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;

    private readonly List<Card> drawPileCards = new List<Card>();
    private readonly List<Card> discardPileCards = new List<Card>();
    private readonly List<Card> handCards = new List<Card>();



    private void OnEnable()
    {
        ActionSystem.AttachPerformer<DrawCardsGA>(DrawCardsPerformer);
        ActionSystem.AttachPerformer<DiscardAllCardsGA>(discardAllCardsPerformer);
        ActionSystem.AttachPerformer<PlayCardGA>(PlayCardPerformer);
        ActionSystem.AttachPerformer<ReHandCardGA>(ReHandCardPerformer);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPreRection, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostRection, ReactionTiming.POST);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<DrawCardsGA>();
        ActionSystem.DetachPerformer<DiscardAllCardsGA>();
        ActionSystem.DetachPerformer<PlayCardGA>();
        ActionSystem.DetachPerformer<ReHandCardGA>();
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPreRection, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostRection, ReactionTiming.POST);
    }

    /// <summary>
    /// 初始化牌组
    /// 把卡牌数据转换成真正的卡牌对象，放入抽牌堆
    /// </summary>
    /// <param name="deckData"></param>
    public void Setup(List<CardData> deckData)
    {
        foreach (var cardData in deckData)
        {
            Card card;

            //关键：判断数据类型，创建对应的卡牌类型
            if (cardData is ActionCardData actionData)
            {
                // 是行动卡数据 → 创建 ActionCard
                card = new ActionCard(actionData);
            }
            else if (cardData is ElementCardData elementCardData)
            {
                card=new ElementCard(elementCardData);
            }
            else
            {
                Debug.Log("为普通牌，初始化错误，错误");
                // 普通卡 → 创建基础 Card
                card = new Card(cardData);
            }
            drawPileCards.Add(card);
        }
    }

    /// <summary>
    /// 抽牌行为执行器
    /// </summary>
    /// <param name="drawCardsGA"></param>
    /// <returns></returns>
    private IEnumerator DrawCardsPerformer(DrawCardsGA drawCardsGA)
    {
        int needDraw = drawCardsGA.Amount;
        for (int i = 0; i < needDraw; i++)
        {
            if (drawPileCards.Count == 0)
            {
                RefillDeck();
                //如果洗牌后还是空的，直接退出
                if (drawPileCards.Count == 0)
                    yield break;
            }
            //执行单张抽牌
            yield return DrawCard();
        }

    }

    /// <summary>
    /// 弃牌器行为执行器
    /// </summary>
    /// <param name="discardAllCardsGA"></param>
    /// <returns></returns>
    private IEnumerator discardAllCardsPerformer(DiscardAllCardsGA discardAllCardsGA)
    {
        if (handCards.Count == 0)
        {
            Debug.Log("discardAllCardsPerformer: 手牌为空，无需弃牌");
            yield break;
        }
        // 复制一份手牌列表，避免遍历时修改原列表
        List<Card> cardsToDiscard = new List<Card>(handCards);

        foreach (Card card in cardsToDiscard)
        {
            // 从手牌视图移除，获取 CardLogic，并取消事件订阅
            CardLogic cardLogic = handHolder.RemoveCard(card);

            if (cardLogic != null)
            {
                // 播放弃牌动画并等待完成（顺序弃牌）
                yield return DiscardCard(cardLogic);
            }

        }
        // hand 已经通过 Remove 清空，无需再 Clear
    }

    /// <summary>
    /// 出牌行为执行器
    /// </summary>
    /// <param name="playCardGA"></param>
    /// <returns></returns>
    private IEnumerator PlayCardPerformer(PlayCardGA playCardGA)
    {
        //手牌列表移除
        handCards.Remove(playCardGA.Card);
        discardPileCards.Add(playCardGA.Card);
        CardLogic cardLogic=handHolder.RemoveCard(playCardGA.Card);
        yield return DiscardCard(cardLogic);
        //打出后效果


        //打出后消耗法力值
        SpendManaGA spendManaGA = new(playCardGA.Card.ManaCost);
        ActionSystem.Instance.AddReaction(spendManaGA);

        //判断是属性牌还是攻击牌
        if (playCardGA.Card is ActionCard actionCard)
        {
            EnemyView target = EnemyBoardView.Instance.EnemyViews[0];
            WeaponActionGA actionGA = new WeaponActionGA(actionCard, target);
            ActionSystem.Instance.AddReaction(actionGA);
        }
        else if (playCardGA.Card is ElementCard elementCard)
        {
            // 属性牌：创建赋能动作，进入弃牌堆，抽1张
            WeaponAddElementGA addElementGA = new WeaponAddElementGA(elementCard);
            ActionSystem.Instance.AddReaction(addElementGA);
        }
        else 
        {
            Debug.Log("PlayCard不对,普通Card基类");

        }

        yield return DrawCardsPerformer(new DrawCardsGA(1));
    }

    public IEnumerator ReHandCardPerformer(ReHandCardGA reHandCardGA)
    {
        List<CardLogic> ReHandCards=handHolder.GetAllSelectedCards();
        int count=ReHandCards.Count;
        if (count == 0)
        {
            Debug.Log("discardAllCardsPerformer: 手牌为空，无需弃牌");
            yield break;
        }
        // 复制一份手牌列表，避免遍历时修改原列表
        List<CardLogic> cardsToDiscard = new List<CardLogic>(ReHandCards);
        foreach (var cardLogic in cardsToDiscard)
        {
            if (cardLogic != null)
            {
                // 播放弃牌动画并等待完成（顺序弃牌）
                yield return DiscardCard(cardLogic);
            }
        }
        yield return DrawCardsPerformer(new DrawCardsGA(count));
    }


    #region 敌人回合前后相关内容
    private void EnemyTurnPreRection(EnemyTurnGA enemyTurnGA)
    {
        DiscardAllCardsGA discardAllCardsGA = new();
        ActionSystem.Instance.AddReaction(discardAllCardsGA);
    }
    private void EnemyTurnPostRection(EnemyTurnGA enemyTurnGA)
    {
        DrawCardsGA drawCardsGA = new(HandAmount);
        ActionSystem.Instance.AddReaction(drawCardsGA);
    }
    #endregion


    #region 工具方法
    /// <summary>
    /// 抽取单张牌
    /// </summary>
    /// <returns></returns>
    private IEnumerator DrawCard()
    {
        //从牌堆中随机抽一张牌并移除（拓展方法）
        Card card = drawPileCards.Draw();
        //添加手牌
        handCards.Add(card);
        // 调用Holder.addCard方法就行        //创建卡牌UI(从牌堆位置生成)
        yield return handHolder.AddCard(card, drawPilePoint);

    }


    /// <summary>
    /// 弃牌堆重新填充到抽牌堆，并且洗牌
    /// </summary>
    private void RefillDeck()
    {
        //把弃牌堆所有牌加入抽牌堆
        drawPileCards.AddRange(discardPileCards);
        //清空弃牌堆
        discardPileCards.Clear();
        //洗牌
        ShuffleList(drawPileCards);
    }

    /// <summary>
    /// 洗牌
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            //生成随机索引
            int randomIndex=Random.Range(0, i+1);
            //交换位置
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    /// <summary>
    /// 弃掉一张卡牌（播放移动到弃牌堆的动画，然后销毁）
    /// </summary>
    private IEnumerator DiscardCard(CardLogic cardLogic)
    {
        if (cardLogic == null) yield break;

        // 【第一步】先从手牌系统移除（必须最先做！）
        handCards.Remove(cardLogic.card);
        discardPileCards.Add(cardLogic.card);
        handHolder.RemoveCard(cardLogic.card); 


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

    #endregion
}
