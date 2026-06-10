using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SynthesisSystem : Singleton<SynthesisSystem>
{
    [SerializeField]private BagCardsHolder BagCardsHolder;
    [SerializeField] private synCardsHolder synCardsHolder;
    public Button btnAddCard;
    public Button btnRemoveCard;
    public Button btnSure;


    List<CardLogic> bagSelectedLogics = new List<CardLogic>();
    List<CardLogic> synSelectedLogics = new List<CardLogic>();

    private List<Card> materialCards=new List<Card>();

    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<synAddCardGA>(synAddCardPerformer);
        ActionSystem.AttachPerformer<synRemoveCardGA>(synRemoveCardPerformer);

    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<synAddCardGA>();
        ActionSystem.DetachPerformer<synRemoveCardGA>();
    }
    public void setup()
    {
        setupBtn();
    }

    private void setupCard()
    {

    }
    private void setupBtn()
    {
        btnAddCard.onClick.AddListener(() =>
        {

            synAddCardGA synAddCardGA = new synAddCardGA();
            ActionSystem.Instance.Perform(synAddCardGA);

        });
        btnRemoveCard.onClick.AddListener(() =>
        {
            synRemoveCardGA synRemoveCardGA = new synRemoveCardGA();
            ActionSystem.Instance.Perform(synRemoveCardGA);
        });
        btnSure.onClick.AddListener(() =>
        {
            //检测是否合成成功
            //成功就删卡
            //加新卡到背包，已经更新数据库
        });
    }

    void GetBagSelectedCards()
    {
        bagSelectedLogics.Clear();
        bagSelectedLogics = BagCardsHolder.GetAllSelectedCards();
        if (bagSelectedLogics.Count == 0)
        {
            Debug.Log("没有选中任何卡牌，无法");
            return;
        }
    }
    void GetSynSelectedCards()
    {
        synSelectedLogics.Clear();
        synSelectedLogics = synCardsHolder.GetAllSelectedCards();
        if (synSelectedLogics.Count == 0)
        {
            Debug.Log("没有选中任何卡牌，无法");
            return;
        }
    }
    void updateBagView()
    {

    }

    private IEnumerator synAddCardPerformer(synAddCardGA synAddCardGA)
    {

        if (materialCards.Count >= 4)
        {
            Debug.LogWarning("田字格已满，最多4张卡牌");
            yield break;
        }
        //获取被选中的卡牌
        GetBagSelectedCards();
        if (bagSelectedLogics == null || bagSelectedLogics.Count == 0 || bagSelectedLogics.Count > 4)
        {
            Debug.Log("选择卡空");
            yield break;
        }

        yield return(bagSystem.Instance.DisBagCardViewPerformer(bagSelectedLogics));

        foreach (var cardLogic in bagSelectedLogics)
        {
            yield return synCardsHolder.AddCard(cardLogic.card, drawPilePoint);
            materialCards.Add(cardLogic.card);
        }

    }

    private IEnumerator synRemoveCardPerformer(synRemoveCardGA synRemoveCardGA )
    {
        //获取被选中的卡牌
        GetSynSelectedCards();
        if (synSelectedLogics == null || synSelectedLogics.Count == 0 || synSelectedLogics.Count > 4)
        {
            yield break;
        }

        List<Card> cards = new List<Card>();
        foreach (var cardLogic in synSelectedLogics)
        {

            yield return DiscardCard(cardLogic);
            cards.Add(cardLogic.card);
            if (materialCards.Contains(cardLogic.card))
                materialCards.Remove(cardLogic.card);
        }

        if (cards.Count > 0)
            yield return bagSystem.Instance.AddBagCardViewPerformer(cards);
    }

    private IEnumerator synSurePerformer(synSureGA synSureGA)
    {
        yield break;
    }


    /// <summary>
    /// 合成核心方法示例
    /// </summary>
    /// <returns></returns>
    public bool TrySynthesize()
    {
        return false;
    }

    //    // 1. 获取当前选中的所有卡牌ID
    //    List<int> selectIds = new List<int>();
    //foreach (var cardLogic in DeckCardsHolder.Instance.selectedCards)
    //{
    //    selectIds.Add(cardLogic.cardData.cardId);
    //}

    //// 2. 去配方表匹配
    //SynthesisRecipe recipe = SynthesisRecipeTable.Instance.MatchRecipe(selectIds);

    //// 3. 判断是否匹配到配方
    //if (recipe == null)
    //{
    //    Debug.Log("所选卡牌没有对应合成配方！");
    //    return;
    //}

    //// 走到这里 = 配方匹配成功
    //int targetCardId = recipe.resultCardId;
    //Debug.Log($"匹配到配方，成品ID：{targetCardId}");
    //Update is called once per frame





    /// <summary>
    /// 销毁synView一张卡牌（播放移动到弃牌堆的动画，然后销毁）
    /// </summary>
    private IEnumerator DiscardCard(CardLogic cardLogic)
    {
        if (cardLogic == null) yield break;

        // 【第一步】先从手牌系统移除（必须最先做！）
        materialCards.Remove(cardLogic.card);

        synCardsHolder.RemoveCard(cardLogic.card);


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
