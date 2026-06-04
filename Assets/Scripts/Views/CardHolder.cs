using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CardHolder : Singleton<HorizontalCardHolder>
{
    //卡牌环境
    private E_CardDisplayContext cardDisplayContext = E_CardDisplayContext.InMath;

    [SerializeField] private CardLogic selectedCardLogic;
    [SerializeReference] private CardLogic hoveredCardLogic;

    [SerializeField] private GameObject slotPrefab;
    private RectTransform rect;

    [Header("Visual Parent")]
    [SerializeField] private Transform visualParent;       // CardVisual 的父物体（通常是VisualHandler）

    [Header("Spawn Settings")]
    //[SerializeField] private int cardsToSpawn = 7;
    public List<CardLogic> cardLogics;

    bool isCrossing = false;
    [SerializeField] private bool tweenCardReturn = true;

    //出牌区域
    [SerializeField] private RectTransform playAreaRect;//拖入场景中的出牌区域

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        //for (int i = 0; i < cardsToSpawn; i++)
        //{
        //    Instantiate(slotPrefab, transform);
        //}
        //List<Card> cards = GetComponentsInChildren<Card>().ToList();

        rect = GetComponent<RectTransform>();

    }
    /// <summary>
    /// 杰哥
    /// </summary>
    /// <param name="cardLogics"></param>
    public void SetCardList(List<CardLogic> cardLogics)
    {
        this.cardLogics = cardLogics;

        int cardCount = 0;
        foreach (CardLogic cardLogic in cardLogics)
        {
            cardLogic.PointerEnterEvent.AddListener(CardPointerEnter);
            cardLogic.PointerExitEvent.AddListener(CardPointerExit);
            cardLogic.BeginDragEvent.AddListener(BeginDrag);
            cardLogic.EndDragEvent.AddListener(EndDrag);
            cardLogic.name = cardCount.ToString();
            cardCount++;
        }

        StartCoroutine(Frame());

        IEnumerator Frame()
        {
            yield return new WaitForSecondsRealtime(.1f);
            for (int i = 0; i < cardLogics.Count; i++)
            {
                if (cardLogics[i].cardVisual != null)
                    cardLogics[i].cardVisual.UpdateIndex(transform.childCount);
            }
        }
    }

    #region 实例化卡牌AddCard
    /// <summary>
    /// 动态添加一张新卡牌（根据 Card）
    /// </summary>
    public IEnumerator AddCard(Card card, Transform drawPilePoint)
    {
        GameObject freeSlot = GetFreeSlot();

        // 使用 Creator 创建卡牌
        CardLogic newCardLogic = CardViewCreator.Instance.CreateCardVisual(card, freeSlot.transform, drawPilePoint, visualParent, cardDisplayContext);

        // 3. 绑定槽位到 CardLogic（关键！后面弃牌要靠它销毁）
        newCardLogic.slotGameObject = freeSlot;



        // 添加到 cards 列表并绑定事件
        RegisterCard(newCardLogic);

        // 更新所有卡牌的视觉索引
        foreach (CardLogic cardLogic in cardLogics)
        {
            if (cardLogic.cardVisual != null)
                cardLogic.cardVisual.UpdateIndex(transform.childCount);
        }

        yield return new WaitForSeconds(0.15f);
    }
    /// <summary>
    /// 杰哥绑定事件
    /// </summary>
    /// <param name="cardLogic"></param>
    public void RegisterCard(CardLogic cardLogic)
    {
        this.cardLogics.Add(cardLogic);

        cardLogic.PointerEnterEvent.AddListener(CardPointerEnter);
        cardLogic.PointerExitEvent.AddListener(CardPointerExit);
        cardLogic.BeginDragEvent.AddListener(BeginDrag);
        cardLogic.EndDragEvent.AddListener(EndDrag);
        cardLogic.name = this.cardLogics.Count.ToString();
    }
    #endregion


    /// <summary>
    /// 移除卡牌
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public CardLogic RemoveCard(Card card)
    {
        CardLogic ReCardLogic = GetCardVlogic(card);
        if (ReCardLogic == null) return null;
        // 添加到 cards 列表并绑定事件
        UnregisterCard(ReCardLogic);


        // 更新所有卡牌的视觉索引
        RefreshCardLayout();

        return ReCardLogic;
    }
    /// <summary>
    /// 强制刷新卡牌容器布局（解决弃牌占位）
    /// </summary>
    public void RefreshCardLayout()
    {
        // 重新更新所有卡牌的索引
        foreach (CardLogic cardLogic in cardLogics)
        {
            if (cardLogic == null || cardLogic.cardVisual == null) continue;
            cardLogic.cardVisual.UpdateIndex(transform.childCount);
        }

    }
    /// <summary>
    /// 杰哥，移除卡牌，取消订阅事件
    /// </summary>
    /// <param name="cardLogic"></param>
    public void UnregisterCard(CardLogic cardLogic)
    {
        this.cardLogics.Remove(cardLogic);

        cardLogic.PointerEnterEvent.RemoveListener(CardPointerEnter);
        cardLogic.PointerExitEvent.RemoveListener(CardPointerExit);
        cardLogic.BeginDragEvent.RemoveListener(BeginDrag);
        cardLogic.EndDragEvent.RemoveListener(EndDrag);
    }
    public CardLogic GetCardVlogic(Card card)
    {
        return cardLogics.Where(cardLogic => cardLogic.card == card).FirstOrDefault();
    }



    /// <summary>
    /// 获取一个空闲槽位（没有子物体），如果没有则创建新槽位
    /// </summary>
    private GameObject GetFreeSlot()
    {
        foreach (Transform slot in transform)
        {
            // 额外检查：如果槽位即将被销毁，也跳过
            if (slot == null || slot.gameObject == null) continue;
            if (slot.childCount == 0)
            {
                return slot.gameObject;
            }
        }
        return Instantiate(slotPrefab, transform);
    }


    private void BeginDrag(CardLogic cardLogic)
    {
        selectedCardLogic = cardLogic;
    }

    void EndDrag(CardLogic cardLogic, Vector2 endScreenPos)
    {
        if (selectedCardLogic == null)
            return;

        // ===== 新增：出牌区域检测 =====
        // 法力值足够,直接调用新的IsOverPlayArea
        //只调用1次，缓存结果（消除重复调用）

        // 拿到UI对应的事件相机(MainCamera)
        // 从父物体Canvas身上拿GraphicRaycaster，再取EventCamera
        GraphicRaycaster raycaster = playAreaRect.GetComponentInParent<GraphicRaycaster>();
        Camera uiCam = raycaster.eventCamera;
        Card card = cardLogic.card;
        // 只计算1次是否在出牌区
        bool isOverPlayArea = playAreaRect != null && RectTransformUtility.RectangleContainsScreenPoint(playAreaRect, endScreenPos, uiCam);

        Debug.Log($"[拖拽结束] 卡牌: {card?.CardName}, 屏幕坐标: {endScreenPos}, 是否在出牌区: {isOverPlayArea}");

        //关键：只有卡牌落在出牌区内，才计算范围、判定左右手
        if (isOverPlayArea && card != null)
        {
            Vector3[] corners = new Vector3[4];
            playAreaRect.GetWorldCorners(corners);
            Vector2 areaLeft = RectTransformUtility.WorldToScreenPoint(uiCam, corners[0]);
            Vector2 areaRight = RectTransformUtility.WorldToScreenPoint(uiCam, corners[2]);
            Debug.Log($"[区域判定] 出牌区屏幕范围: X({areaLeft.x:F2} ~ {areaRight.x:F2}),  卡牌X: {endScreenPos.x:F2}");
        }

        // 业务分发（注意：这里不要再调用任何归位动画）
        if (isOverPlayArea)
        {
            if (card is WeaponCard weaponCard)
            {
                //显示装备武器牌提示框，是否装备
                Debug.Log("请装备武器");
            }
            else if (card is ActionCard actionCard|| card is ElementCard elementCard)
            {
                //扣钱
                //加到手牌中
            }
            else
            {
                Debug.Log("普通卡牌商店错误，无法使用的卡牌类型");
            }
        }




        selectedCardLogic.transform.DOLocalMove(selectedCardLogic.selected ? new Vector3(0, selectedCardLogic.selectionOffset, 0) : Vector3.zero, tweenCardReturn ? .15f : 0).SetEase(Ease.OutBack);

        rect.sizeDelta += Vector2.right;
        rect.sizeDelta -= Vector2.right;

        selectedCardLogic = null;
    }




    void CardPointerEnter(CardLogic cardLogic)
    {
        hoveredCardLogic = cardLogic;
    }

    void CardPointerExit(CardLogic cardLogic)
    {
        hoveredCardLogic = null;
    }

    void Update()
    {
        //新增：先把列表里已经被销毁的卡牌清掉（只加这一行）
        cardLogics.RemoveAll(card => card == null);
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (hoveredCardLogic != null)
            {
                Destroy(hoveredCardLogic.transform.parent.gameObject);
                cardLogics.Remove(hoveredCardLogic);

            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            foreach (CardLogic cardLogic in cardLogics)
            {
                cardLogic.Deselect();
            }
        }

        if (selectedCardLogic == null)
            return;

        if (isCrossing)
            return;
        for (int i = 0; i < cardLogics.Count; i++)
        {

            if (selectedCardLogic.transform.position.x > cardLogics[i].transform.position.x)
            {
                if (selectedCardLogic.ParentIndex() < cardLogics[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }

            if (selectedCardLogic.transform.position.x < cardLogics[i].transform.position.x)
            {
                if (selectedCardLogic.ParentIndex() > cardLogics[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }
        }
    }

    void Swap(int index)
    {
        isCrossing = true;

        // 拿到两张要交换的卡牌
        CardLogic currentCard = selectedCardLogic;
        CardLogic targetCard = cardLogics[index];

        // 拿到它们现在的卡槽
        Transform currentSlot = currentCard.transform.parent;
        Transform targetSlot = targetCard.transform.parent;

        Transform focusedParent = selectedCardLogic.transform.parent;
        Transform crossedParent = cardLogics[index].transform.parent;

        cardLogics[index].transform.SetParent(focusedParent);
        cardLogics[index].transform.localPosition = cardLogics[index].selected ? new Vector3(0, cardLogics[index].selectionOffset, 0) : Vector3.zero;
        selectedCardLogic.transform.SetParent(crossedParent);

        // 【关键修复】交换后 实时更新卡槽绑定！！！
        currentCard.slotGameObject = targetSlot.gameObject;
        targetCard.slotGameObject = currentSlot.gameObject;

        isCrossing = false;

        if (cardLogics[index].cardVisual == null)
            return;

        bool swapIsRight = cardLogics[index].ParentIndex() > selectedCardLogic.ParentIndex();
        cardLogics[index].cardVisual.Swap(swapIsRight ? -1 : 1);

        //Updated Visual Indexes
        foreach (CardLogic cardLogic in cardLogics)
        {
            cardLogic.cardVisual.UpdateIndex(transform.childCount);
        }
    }

    /// <summary>
    /// 返回当前选中上浮的卡牌
    /// </summary>
    /// <returns></returns>
    public List<CardLogic> GetAllSelectedCards()
    {
        List<CardLogic> list = new List<CardLogic>();
        foreach (var card in cardLogics)
        {
            if (card != null && card.selected)
            {
                list.Add(card);
            }
        }
        return list;
    }
}
