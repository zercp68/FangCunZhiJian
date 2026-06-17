using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPanelSystem : Singleton<WeaponPanelSystem>
{
    [SerializeField] private WeaCardsHolder weaBagCardsHolder;

    [SerializeField] private WeaponCard currentLeftWeapon;
    [SerializeField] private WeaponCard currentRightWeapon;

    [SerializeField] private WeaponView LeftWeaponView;
    [SerializeField] private WeaponView RightWeaponView;

    [SerializeField] private Transform LeftDrawPilePoint;
    [SerializeField] private Transform RightDrawPilePoint;
    [SerializeField] private Transform BagCardsHolderDrawPilePoint;
    [SerializeField] private Transform LeftDiscardPilePoint;
    [SerializeField] private Transform RightDiscardPilePoint;
    [SerializeField] private Transform BagCardsHolderDiscardPilePoint;


    private List<WeaponCard> currentWeaBagCards = new List<WeaponCard>();
    private CardLogic selectedWeaCard;
    private List<CardLogic> selectedWeaCards;

    public Text txtLeftDes;
    public Text txtRightDes;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<DrawWeaBagGA>(DrawWeaBagPerformer);
        ActionSystem.AttachPerformer<ChangeWeaGA>(ChangeWeaPerformer);
        ActionSystem.AttachPerformer<UnEquipWeaponGA>(UnEquipWeaponPerformer);
        ActionSystem.AttachPerformer<SellWeaponCardsGA>(SellWeaponCardPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<DrawWeaBagGA>();
        ActionSystem.DetachPerformer<ChangeWeaGA>();
        ActionSystem.DetachPerformer<UnEquipWeaponGA>();
        ActionSystem.DetachPerformer<SellWeaponCardsGA>();
    }





    public void setup()
    {
        setupWeapon();
        setupWeaBagCards();
    }

    void setupWeapon()
    {
        currentLeftWeapon = PlayerDataManager.Instance.CurrentLeftWeapon;
        currentRightWeapon = PlayerDataManager.Instance.CurrentRightWeapon;
    }

    void setupWeaBagCards()
    {
        currentWeaBagCards = new List<WeaponCard>();
        if (PlayerDataManager.Instance != null)
        {
            List<WeaponCard> deckCopy = PlayerDataManager.Instance.GetCurrentWeaponDeckCopy();
            currentWeaBagCards.AddRange(deckCopy); // 赋值核心逻辑（也可直接 deckCards = deckCopy;）
            Debug.Log($"MatchSetup: 从PlayerDataManager获取到卡组数量: {deckCopy.Count}"); // 新增日志
        }
        else
        {
            Debug.LogError("PlayerDataManager.Instance 为空，请检查是否挂载该单例！");
        }
    }

    /// <summary>
    /// 装备的武器视图执行器
    /// </summary>
    /// <returns></returns>
    public IEnumerator UpdateWeaView(E_WeaponHand e_WeaponHand)
    {
        switch (e_WeaponHand)
        {
            case E_WeaponHand.Left:
                //建立武器视图
                if (currentLeftWeapon != null)
                {
                    yield return LeftWeaponView.creatWeapon(currentLeftWeapon);
                }
                break;
            case E_WeaponHand.Right:
                //建立武器视图
                if (currentRightWeapon != null)
                {
                    yield return RightWeaponView.creatWeapon(currentRightWeapon);
                }
                break;
        }
        //写des文字
        yield return updateWeaDes(txtLeftDes, currentLeftWeapon);
        yield return updateWeaDes(txtRightDes, currentRightWeapon);
        yield break;
    }


    /// <summary>
    /// 画背包武器卡牌
    /// </summary>
    /// <param name="DrawWeaBagGA"></param>
    /// <returns></returns>
    public IEnumerator DrawWeaBagPerformer(DrawWeaBagGA DrawWeaBagGA)
    {
        // 调用Holder.addCard方法就行        //创建卡牌UI(从牌堆位置生成)
        foreach(var card in currentWeaBagCards)
        {
            yield return weaBagCardsHolder.AddCard(card, BagCardsHolderDrawPilePoint);
        }
        yield break;
    }
    public IEnumerator ChangeWeaPerformer(ChangeWeaGA changeWeaGA)
    {

        GetBagSelectedCard();
        switch (changeWeaGA.E_WeaponHand)
        {
            case E_WeaponHand.None:
                break;
            case E_WeaponHand.Left:
                //当前武器不为空时
                if (currentLeftWeapon != null)
                {
                    currentLeftWeapon.IsWeaponEquipped = false;
                    UnEquipWeaponGA unEquipWeaponGA = new UnEquipWeaponGA(E_WeaponHand.Left);
                    ActionSystem.Instance.AddReaction(unEquipWeaponGA);
                }
                currentLeftWeapon = selectedWeaCard.card as WeaponCard;
                if (currentLeftWeapon!=null)
                {
                    yield return DiscardWeaBagCard(selectedWeaCard);
                    currentLeftWeapon.IsWeaponEquipped = true;
                    PlayerDataManager.Instance.EquipLeftWeapon(currentLeftWeapon);
                    yield return UpdateWeaView(E_WeaponHand.Left);
                }
                break;  
            case E_WeaponHand.Right:
                // 缺失：先卸装原有右手武器
                if (currentRightWeapon != null)
                {
                    currentRightWeapon.IsWeaponEquipped = false;

                    UnEquipWeaponGA unEquipWeaponGA = new UnEquipWeaponGA(E_WeaponHand.Right,currentRightWeapon);
                    ActionSystem.Instance.AddReaction(unEquipWeaponGA);
                }
                currentRightWeapon = selectedWeaCard.card as WeaponCard;
                if (currentRightWeapon != null)
                {
                    yield return DiscardWeaBagCard(selectedWeaCard);
                    currentRightWeapon.IsWeaponEquipped = true;
                    PlayerDataManager.Instance.EquipRightWeapon(currentRightWeapon);
                    yield return UpdateWeaView(E_WeaponHand.Right);
                }
                break;
        }
        yield break;
    }
    void GetBagSelectedCard()
    {
        selectedWeaCard = weaBagCardsHolder.GetSelectedCard();
        if (selectedWeaCard == null)
        {
            Debug.Log("没有选中任何卡牌，无法");
            return;
        }
    }
    public IEnumerator UnEquipWeaponPerformer(UnEquipWeaponGA unEquipWeaponGA)
    {
        WeaponCard weaponCard;
        switch (unEquipWeaponGA.E_WeaponHand)
        {
            case E_WeaponHand.Left:
                weaponCard = currentLeftWeapon;
                //清空武器卡牌视图
                yield return DiscardWeaView(E_WeaponHand.Left);
                //在武器卡牌背包视图加上当前武器牌
                yield return weaBagCardsHolder.AddCard(weaponCard, LeftDrawPilePoint);

                yield return UpdateWeaView(E_WeaponHand.Left);
                //武器背包加上，然后删掉当前装备
                PlayerDataManager.Instance.AddCardToWeaponDeck(weaponCard);
                PlayerDataManager.Instance.UnequipLeftWeapon();
                break;
            case E_WeaponHand.Right:
                weaponCard = currentRightWeapon;
                //清空武器卡牌视图
                yield return DiscardWeaView(E_WeaponHand.Right);
                //在武器卡牌背包视图加上当前武器牌
                yield return weaBagCardsHolder.AddCard(weaponCard, RightDrawPilePoint);
                yield return UpdateWeaView(E_WeaponHand.Left);
                //武器背包加上，然后删掉当前装备
                PlayerDataManager.Instance.AddCardToWeaponDeck(weaponCard);
                PlayerDataManager.Instance.UnequipRightWeapon();
                break;
        }


        yield break;
    }


    void GetBagSelectedCards()
    {
        selectedWeaCards = weaBagCardsHolder.GetAllSelectedCards();
        if (selectedWeaCards.Count==0)
        {
            Debug.Log("没有选中任何卡牌，无法");
            return;
        }
    }
    public IEnumerator SellWeaponCardPerformer(SellWeaponCardsGA sellWeaponCardsGA)
    {
        selectedWeaCards = new List<CardLogic>();
        GetBagSelectedCards();
        if (selectedWeaCards.Count == 0)
        {
            Debug.Log("没有选择卡");
            yield break;
        }
        foreach(var cardLogic in selectedWeaCards)
        {
            WeaponCard weaponCard = cardLogic.card as WeaponCard;
            PlayerDataManager.Instance.SellWeaponCard(weaponCard);
            yield return DiscardWeaBagCard(cardLogic);
        }
    }


    /// <summary>
    /// 更新武器视图描述的迭代器    
    /// </summary>
    /// <param name="txtDes"></param>
    /// <param name="weaponCard"></param>
    /// <returns></returns>
    /// 
    private IEnumerator updateWeaDes(Text txtDes, WeaponCard weaponCard)
    {
        if (weaponCard == null)
        {
            txtDes.text = "未装备武器";
            yield break;
        }
        txtDes.text = weaponCard.Description;
    }




    /// <summary>
    /// 从卡牌背包弃掉一张卡牌（播放移动到弃牌堆的动画，然后销毁）
    /// </summary>
    private IEnumerator DiscardWeaBagCard(CardLogic cardLogic)
    {
        if (cardLogic == null) yield break;


        // 【第一步】先从手牌系统移除（必须最先做！）
        WeaponCard weaponCard = cardLogic.card as WeaponCard;

        currentWeaBagCards.Remove(weaponCard);

        weaBagCardsHolder.RemoveCard(cardLogic.card);


        // 【第二步】播放动画
        if (cardLogic.cardVisual != null)
        {
            Transform visualTransform = cardLogic.cardVisual.transform;
            Vector3 targetWorldPos = BagCardsHolderDiscardPilePoint.position;

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


    /// <summary>
    /// 删除武器卡牌（播放移动到弃牌堆的动画，然后销毁）
    /// </summary>
    private IEnumerator DiscardWeaView(E_WeaponHand e_WeaponHand)
    {
        // 1. 初始化变量并获取对应武器视图的CardLogic
        CardLogic cardLogic = null;
        WeaponView targetWeaponView = null;

        switch (e_WeaponHand)
        {
            case E_WeaponHand.Left:
                targetWeaponView = LeftWeaponView;
                break;
            case E_WeaponHand.Right:
                targetWeaponView = RightWeaponView;
                break;
        }
        // 2. 空值校验：视图或卡牌为空则直接退出
        if (targetWeaponView == null)
        {
            Debug.LogError($"DiscardWeaView: {e_WeaponHand} 武器视图未赋值");
            yield break;
        }
        cardLogic = targetWeaponView.WeaCardLogic;
        if (cardLogic == null)
        {
            Debug.LogWarning($"DiscardWeaView: {e_WeaponHand} 武器视图无卡牌，无需销毁");
            yield break;
        }

        // 只声明一次目标坐标（修复重复定义报错）
        Vector3 targetWorldPos = Vector3.zero;
        // 【第二步】播放动画
        if (cardLogic.cardVisual != null)
        {
            Transform visualTransform = cardLogic.cardVisual.transform;
            switch (e_WeaponHand)
            {
                case E_WeaponHand.Left:
                    targetWorldPos = LeftDiscardPilePoint.position;
                    break;
                case E_WeaponHand.Right:
                    targetWorldPos = RightDiscardPilePoint.position;
                    break;
            }


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

        switch (e_WeaponHand)
        {
            case E_WeaponHand.Left:
                currentLeftWeapon = null;
                break;
            case E_WeaponHand.Right:
                currentRightWeapon = null;
                break;
        }

    }
}
