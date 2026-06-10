using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class synCardsHolder : MonoBehaviour
{
    private E_CardDisplayContext cardDisplayContext = E_CardDisplayContext.InMath;
    public CardLogic selectedCardLogic;
    public CardLogic hoveredCardLogic;

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


    [Header("田字格 上下行容器")]
    [SerializeField] private Transform oneCardRow;  //一张卡牌居中
    [SerializeField] private Transform twoCardRow;  //一张卡牌居中
    [SerializeField] private Transform topRow;     // 上排容器 TopRow
    [SerializeField] private Transform bottomRow;  // 下排容器 BottomRow

    // 田字格最大卡牌数量
    private const int MaxCardCount = 4;

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {

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
            //cardLogic.BeginDragEvent.AddListener(BeginDrag);
            //cardLogic.EndDragEvent.AddListener(EndDrag);
            cardLogic.name = cardCount.ToString();
            cardCount++;
        }

        // 初始化布局
        RearrangeGrid();
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
        Debug.Log($"[AddCard] 进入添加方法，当前卡牌数量：{cardLogics.Count}");

        if (cardLogics.Count >= MaxCardCount)
        {
            Debug.LogWarning("田字格已满，最多4张卡牌");
            yield break;
        }

        GameObject freeSlot = GetFreeSlot();
        if (freeSlot == null)
        {
            Debug.LogError("[AddCard] 获取空闲槽位失败，freeSlot = null");
            yield break;
        }
        Debug.Log($"[AddCard] 获取到空闲槽位：{freeSlot.name}");

        CardLogic newCardLogic = CardViewCreator.Instance.CreateCardVisual(card, freeSlot.transform, drawPilePoint, visualParent, cardDisplayContext);
        if (newCardLogic == null)
        {
            Debug.LogError("[AddCard] 创建 CardLogic 失败，返回空");
            yield break;
        }
        Debug.Log($"[AddCard] 卡牌创建成功：{newCardLogic.name}");

        newCardLogic.slotGameObject = freeSlot;
        newCardLogic.canDrag = false;

        RegisterCard(newCardLogic);
        Debug.Log($"[AddCard] 卡牌注册完成，当前列表总数：{cardLogics.Count}");

        
        RearrangeGrid();
        RefreshCardLayout();

        yield return new WaitForSeconds(0.15f);
    }
    /// <summary>
    /// 杰哥绑定事件
    /// </summary>
    /// <param name="cardLogic"></param>
    public void RegisterCard(CardLogic cardLogic)
    {
 
        if (cardLogic == null) return;
        // 先移除再添加，防止重复注册
        cardLogic.PointerEnterEvent.RemoveListener(CardPointerEnter);
        cardLogic.PointerEnterEvent.AddListener(CardPointerEnter);

        cardLogic.PointerExitEvent.RemoveListener(CardPointerExit);
        cardLogic.PointerExitEvent.AddListener(CardPointerExit);

        // 其他监听...
        if (!cardLogics.Contains(cardLogic)) // 防止重复添加
            cardLogics.Add(cardLogic);
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
        // 删卡后重排
        RearrangeGrid();
        return ReCardLogic;
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
        int total = cardLogics.Count;
        Transform targetRow = GetTargetRowByCount(total);

        // 优先复用空闲槽位
        foreach (Transform slot in targetRow)
        {
            if (slot != null && slot.childCount == 0)
                return slot.gameObject;
        }
        // 新建槽位
        return Instantiate(slotPrefab, targetRow);
    }

    /// <summary>
    /// 根据卡牌总数，获取对应容器（规则：1→单卡行 / 2→上排 / 3、4→上+下）
    /// </summary>
    private Transform GetTargetRowByCount(int total)
    {
        return total switch
        {
            0 => oneCardRow,
            1 => twoCardRow,
            2 => bottomRow,
            3 => bottomRow,
            _ => null
        };
    }
    /// <summary>
    /// 全局田字格重排：所有卡牌统一归位到对应行（增/删/交换 必调用）
    /// </summary>
    private void RearrangeGrid()
    {
        int total = cardLogics.Count;
        for (int i = 0; i < cardLogics.Count; i++)
        {
            CardLogic card = cardLogics[i];
            if (card == null) continue;

            Transform targetRow;
            // 最终排布规则
            if (total == 1)
            {
                targetRow = oneCardRow;
            }
            else if (total == 2)
            {
                targetRow = twoCardRow;
                
            }
            else if(total==3)// 3 / 4 张：前2个上排，后2个下排
            {
                
                targetRow = i < 1 ? topRow : bottomRow;
            }
            else 
            {
                targetRow = i < 2 ? topRow : bottomRow;
            }

            // 更换父物体 + 刷新槽位绑定
            card.slotGameObject.transform.SetParent(targetRow, false);
            card.transform.localPosition = Vector3.zero;
        }
    }

    public void RefreshCardLayout()
    {
        foreach (CardLogic cardLogic in cardLogics)
        {
            if (cardLogic == null || cardLogic.cardVisual == null) continue;
            cardLogic.cardVisual.UpdateIndex(transform.childCount);
        }
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

        Debug.Log($"[拖拽结束] 卡牌: {card?.CardId}, 屏幕坐标: {endScreenPos}, 是否在出牌区: {isOverPlayArea}");

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
            else if (card is ActionCard actionCard || card is ElementCard elementCard)
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

        targetCard.transform.SetParent(currentSlot);
        targetCard.transform.localPosition = targetCard.selected ? new Vector3(0, targetCard.selectionOffset, 0) : Vector3.zero;
        currentCard.transform.SetParent(targetSlot);

        // 更新槽位引用
        currentCard.slotGameObject = targetSlot.gameObject;
        targetCard.slotGameObject = currentSlot.gameObject;

        isCrossing = false;

        if (targetCard.cardVisual != null)
        {
            bool swapIsRight = targetCard.ParentIndex() > currentCard.ParentIndex();
            targetCard.cardVisual.Swap(swapIsRight ? -1 : 1);
        }

        RefreshCardLayout();
        // 交换后重排
        RearrangeGrid();
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
