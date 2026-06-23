using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SynthesisSystem : Singleton<SynthesisSystem>
{
    [SerializeField]private BagCardsHolder BagCardsHolder;
    [SerializeField] private synCardsHolder synCardsHolder;
    [SerializeField] private GameObject sythesisCardVFX;
    public Button btnAddCard;
    public Button btnRemoveCard;
    public Button btnSure;


    List<CardLogic> bagSelectedLogics = new List<CardLogic>();
    List<CardLogic> synSelectedLogics = new List<CardLogic>();

    private List<Card> materialCards=new List<Card>();

    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;

    private bool isSynthsis=false;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<synAddCardGA>(synAddCardPerformer);
        ActionSystem.AttachPerformer<synRemoveCardGA>(synRemoveCardPerformer);
        ActionSystem.AttachPerformer<synSureGA>(synSurePerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<synAddCardGA>();
        ActionSystem.DetachPerformer<synRemoveCardGA>();
        ActionSystem.DetachPerformer<synSureGA>();
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
            synSureGA synSureGA = new synSureGA();
            ActionSystem.Instance.Perform(synSureGA);
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


    private IEnumerator synAddCardPerformer(synAddCardGA synAddCardGA)
    {

        if (materialCards.Count >= 4)
        {
            TipPanel tipPanel = UIManager.Instance.ShowPanel<TipPanel>();
            tipPanel.ChangeInfo("田字格已满，最多4张卡牌");
            yield break;
        }
        //获取被选中的卡牌
        GetBagSelectedCards();
        if (bagSelectedLogics == null || bagSelectedLogics.Count == 0 || bagSelectedLogics.Count > 4)
        {
            TipPanel tipPanel = UIManager.Instance.ShowPanel<TipPanel>();
            tipPanel.ChangeInfo("选择的卡牌不能大于4张卡牌");
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
        Debug.Log("【移除卡牌流程 开始】");
        //获取被选中的卡牌
        GetSynSelectedCards();
        if (synSelectedLogics == null || synSelectedLogics.Count == 0 )
        {
            yield break;
        }

        if( synSelectedLogics.Count > 4)
        {
            TipPanel tipPanel = UIManager.Instance.ShowPanel<TipPanel>();
            tipPanel.ChangeInfo("选择的卡牌不能大于4张卡牌");
            yield break;
        }

        List<Card> cards = new List<Card>();
        foreach (var cardLogic in synSelectedLogics)
        {
            yield return DiscardCard(cardLogic);
            cards.Add(cardLogic.card);
        }
        // 统一移除材料（遍历完再删，避免遍历集合修改）
        foreach (var c in cards)
        {
            materialCards.Remove(c);
            Debug.Log($"【从临时材料移除】卡牌{c.CardId}");
        }
        if (cards.Count > 0)
        {
            yield return bagSystem.Instance.AddBagCardViewPerformer(cards, isSynthsis);
            isSynthsis = false;
            Debug.Log("【放回背包完成】isSynthsis重置为false");
        }
        Debug.Log("【移除卡牌流程 结束】");
    }

    private IEnumerator synSurePerformer(synSureGA synSureGA)
    {
        Debug.Log($"【合成开始】当前材料列表materialCards数量：{materialCards.Count}");

        // 1. 获取当前选中卡牌

        if (materialCards.Count == 0)
        {
            TipPanel tipPanel = UIManager.Instance.ShowPanel<TipPanel>();
            tipPanel.ChangeInfo("请先选择合成材料卡牌");
            yield break;
        }

        // 3. 匹配配方
        var targetRecipe = SynthesisRecipeTable.Instance.GetMatchRecipe(materialCards);

        if (targetRecipe == null)
        {
            TipPanel tipPanel = UIManager.Instance.ShowPanel<TipPanel>();
            tipPanel.ChangeInfo("当前选择卡牌无对应合成配方");
            yield break;
        }


        // ========== 关键修复：先拷贝一份集合，遍历副本 ==========
        List<CardLogic> copeCardLogics = new List<CardLogic>(synCardsHolder.cardLogics);

        // 5. 移除材料卡牌（遍历副本，原集合不会被遍历）
        foreach (var cardLogic in copeCardLogics)
        {
            if (cardLogic == null) Debug.Log("kong");
            //对playerDataManager移除卡牌
            PlayerDataManager.Instance.RemoveCardFromDeck(cardLogic.card);
            yield return DiscardCard(cardLogic);
        }
        materialCards.Clear();
        Debug.Log("【材料清空】materialCards已全部清空");
        // 6. 根据【产出卡牌ID】创建新卡牌
        Card newCard = CardManager.Instance.CreateCardById(targetRecipe.outputCard.cardId);
        if (newCard == null)
        {
            Debug.LogError($"根据ID {targetRecipe.outputCard.cardId} 未找到对应卡牌");
            yield break;
        }
        GameObject vfx = Instantiate(sythesisCardVFX, drawPilePoint.transform.position, Quaternion.identity);
        Destroy(vfx, 1f); // 自动销毁
        yield return synCardsHolder.AddCard(newCard, drawPilePoint);
        materialCards.Add(newCard);
        Debug.Log($"【产物存入临时材料列表】materialCards当前数量：{materialCards.Count}");


        // 7. 新卡牌加入玩家卡组
        PlayerDataManager.Instance.AddCardToDeck(newCard);
        Debug.Log($"【PlayerData永久添加产物】卡牌{newCard.CardId} 完成");
        isSynthsis = true;
        Debug.Log($"合成成功！产出卡牌ID：{isSynthsis}");
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
