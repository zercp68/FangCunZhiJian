using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

enum E_bagCardType
{
    allCard,
    actCard,
    eleCard,
    weaCard,
    comCard
}

public class bagSystem : Singleton<bagSystem>
{

    [Header("toggle")]
    public Toggle dotPrefab;
    public List<Toggle> dots;
    public ToggleGroup toggleGroup;

    public Button btnPre;              // 上一页按钮
    public Button btnNext;            // 下一页按钮
    public Button btnAllCard;
    public Button btnActCard;
    public Button btnEleCard;
    public Button btnComCard;
    public Button btnWeaCard;

    [Header("设置")]
    public BagCardsHolder BagCardsHolder;
    private List<Card> currentCards;
    private List<Card> AllCards;
    private List<Card> ActCards;
    private List<Card> EleCards;
    private List<Card> ComCards;
    private List<Card> WeaCards;


    [Header("配置")]
    private int nowTotalCardCount;    // 现在总共有多少个物品（用来计算需要多少页）
    public int itemsPerPage = 16;      // 每页放多少个Card
    private E_bagCardType e_NowBagCardType = E_bagCardType.allCard;
    private int currentPageIndex=0;
    private int maxPageCount;

    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;

    // 全局翻页锁
    private bool _isPageChanging = false;
    private const float LockDuration = 0.2f;

    private Coroutine currentUpdateCoroutine=null;   // 记录当前更新协程
    private bool isCancelling = false;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<UpdatePanelGA>(UpdatePanelPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<UpdatePanelGA>();

    }



    public void setUP()
    {
        //获取所有手牌卡牌
        setupCards();
        setupBtnCard();
        //将当前牌初始化为当前所有手牌
        updateCurrentCards(AllCards);
        //给页数，初始化toggle数量
        GenerateDots();
        // 绑定分页按钮事件
        setupBtnPage();
        // 初始化打开第一页
        SwitchPage(0);
    }



    /// <summary>
    /// 得到当前所有手牌卡牌
    /// </summary>
    public void setupCards()
    {
        // 强制清空+重新初始化，确保数据最新
        AllCards = new List<Card>();
        ActCards = new List<Card>();
        EleCards = new List<Card>();
         ComCards= new List<Card>();
         WeaCards= new List<Card>();
        // 3. 安全获取当前卡组副本并赋值（增加空引用检查）
        if (PlayerDataManager.Instance != null)
        {
            List<Card> deckCopy = PlayerDataManager.Instance.GetCurrentAllCardsCopy();
            AllCards.AddRange(deckCopy); // 赋值核心逻辑（也可直接 deckCards = deckCopy;）
            CardManager.Instance.SplitCardsByType(AllCards, ActCards, EleCards, ComCards,WeaCards);
            Debug.Log($"MatchSetup: 从PlayerDataManager获取到卡组数量: {deckCopy.Count}"); // 新增日志
        }
        else
        {
            Debug.LogError("PlayerDataManager.Instance 为空，请检查是否挂载该单例！");
        }
    }

    private IEnumerator UpdatePanelPerformer(UpdatePanelGA updatePanelGA)
    {
        // 停止旧协程（防御）
        if (currentUpdateCoroutine != null)
            StopCoroutine(currentUpdateCoroutine);

        currentUpdateCoroutine = StartCoroutine(InternalUpdate(updatePanelGA));
        yield return currentUpdateCoroutine;
        currentUpdateCoroutine = null;
    }

    private IEnumerator InternalUpdate(UpdatePanelGA updatePanelGA)
    {
        SetAllInteractable(false);
        yield return ClearAllCard();               // 注意：这里使用有动画的清空，会被中断快速清理替代

        int safeBegin = Mathf.Max(0, updatePanelGA.beginIndex);
        int safeEnd = Mathf.Min(updatePanelGA.endIndex, currentCards.Count);
        if (safeBegin >= currentCards.Count)
        {
            ResetPageLock();
            yield break;
        }
        for (int i = safeBegin; i < safeEnd; i++)
        {
            if (i < 0 || i >= currentCards.Count) continue;
            Card card = currentCards[i];
            yield return BagCardsHolder.AddCard(card, drawPilePoint);
        }
        Invoke(nameof(ResetPageLock), LockDuration);
    }
    // 在 bagSystem 中添加：
    public void CancelCurrentUpdate()
    {
        if (isCancelling) return;
        isCancelling = true;
        if (currentUpdateCoroutine != null)
            StopCoroutine(currentUpdateCoroutine);
        StartCoroutine(ClearAllCardImmediate());
        SetAllInteractable(true);
        _isPageChanging = false;
        isCancelling = false;
    }

    /// <summary>
    /// 立即清空所有卡牌（无动画，用于中断）
    /// </summary>
    private IEnumerator ClearAllCardImmediate()
    {
        foreach (var cardLogic in BagCardsHolder.cardLogics)
        {
            if (cardLogic == null) continue;
            if (cardLogic.cardVisual != null)
                Destroy(cardLogic.cardVisual.gameObject);
            if (cardLogic.slotGameObject != null)
                Destroy(cardLogic.slotGameObject);
            Destroy(cardLogic.gameObject);
        }
        BagCardsHolder.cardLogics.Clear();
        BagCardsHolder.selectedCardLogic = null;
        BagCardsHolder.hoveredCardLogic = null;
        yield return null;
    }

    /// <summary> 
    /// 刷新背包卡牌视图（重读数据+保留当前分类/页码）
    /// </summary>
    public IEnumerator RefreshBagView()
    {
        if (_isPageChanging) yield break;

        _isPageChanging = true;
        SetAllInteractable(false);

        // 重新读取卡组并分类
        setupCards();
        // 刷新当前页面
        RefreshCurrentBagView();

        yield return new WaitForSeconds(LockDuration);
        ResetPageLock();

        UpdatePanelGA firstPage = new UpdatePanelGA(0, Mathf.Min(itemsPerPage, currentCards.Count));
        ActionSystem.Instance.AddReaction(firstPage);
    }


    /// <summary> 批量弃牌 / 移除卡牌 协程 </summary>
    public IEnumerator DisBagCardViewPerformer(List<CardLogic> cardLogics)
    {
        if (_isPageChanging) yield break;

        _isPageChanging = true;
        SetAllInteractable(false);
        // 关键：记录「弃牌前当前页的显示数量」（而非总卡牌数）
        int lastShowCardNum = Mathf.Min(currentCards.Count, itemsPerPage);
        int discardCardNum = 0;
        foreach (var cardLogic in cardLogics)
        {
            if (cardLogic == null) continue;
            discardCardNum++;
            AllCards.Remove(cardLogic.card);
            yield return DiscardCard(cardLogic);
        }

        // 重新拆分卡组
        CardManager.Instance.SplitCardsByType(AllCards, ActCards, EleCards, ComCards, WeaCards);
        setCurrentCards();
        GenerateDots();
        yield return null;
        setupBtnPage();

        // 关键：判断条件改为「弃牌前当前页是满的（>=每页上限）」
        if (lastShowCardNum >= itemsPerPage)
        {
            yield return AddBackcards(discardCardNum);
        }
        yield return new WaitForSeconds(LockDuration);
        ResetPageLock();
    }

    /// <summary> 将后面的牌补进当前页 </summary>
    public IEnumerator AddBackcards(int discardCardNum)
    {
        // 移除错误的状态锁拦截（外层已经锁了，无需重复拦截）
        // if (_isPageChanging) yield return null; 

        // 1. 基础校验
        if (discardCardNum <= 0 || currentCards == null || currentCards.Count == 0)
        {
            Debug.LogWarning("无卡牌可补：弃牌数为0或当前卡组为空");
            yield break;
        }

        // 2. 复用配置的每页数量，而非硬编码16
        int pageMax = itemsPerPage;


        // 3. 正确计算补牌下标（核心修复）
        // 当前页已显示的卡牌数 = 弃牌后当前页剩余数（当前页显示上限 - 弃牌数）
        int currentShowCount = pageMax - discardCardNum;
        // 补牌起始下标：当前页最后一张的下一位（比如当前显示12张，从12开始补）
        int fillStart = currentShowCount;
        if (fillStart >= currentCards.Count) yield break; // 新增：无卡可补直接返回
        // 补牌结束下标：不超过总卡牌数、不超过单页上限
        int fillEnd = Mathf.Min(fillStart + discardCardNum, currentCards.Count);
        fillEnd = Mathf.Min(fillEnd, pageMax); // 兜底：不超过单页上限

        // 4. 边界校验：无牌可补
        if (fillStart >= fillEnd)
        {
            Debug.Log("无额外卡牌可补：已达卡组末尾/单页上限");
            yield break;
        }

        // 5. 循环补牌（带越界保护）
        for (int i = fillStart; i < fillEnd; i++)
        {
            if (i < 0 || i >= currentCards.Count)
            {
                Debug.LogWarning($"补牌下标越界：i={i}，总卡牌数={currentCards.Count}");
                break;
            }

            Card card = currentCards[i];
            yield return BagCardsHolder.AddCard(card, drawPilePoint);
            Debug.Log($"补牌成功：卡牌={card.CardId}，下标={i}");
        }
        yield return null;
    }

    /// <summary> 添加单张卡牌协程 </summary>
    public IEnumerator AddBagCardViewPerformer(List<Card> cards,bool isSynthesis)
    {
        // 防重复执行
        if (_isPageChanging) yield break;

        _isPageChanging = true;
        SetAllInteractable(false);

        if (isSynthesis)
        {
            // 合成场景：重新从PlayerDataManager加载全量卡牌（保证数据源最新）
            setupCards();
            yield return null;
            Debug.Log($"[AddBagCardView] 合成场景，重新加载全量卡牌，总数：{AllCards.Count}");
        }
        else
        {
            // 非合成场景：增量添加卡牌
            AllCards.AddRange(cards);
            // 重新拆分卡牌类型（保证分类数据同步）
            CardManager.Instance.SplitCardsByType(AllCards, ActCards, EleCards, ComCards, WeaCards);
            Debug.Log($"[AddBagCardView] 增量添加卡牌 {cards.Count} 张，全量卡牌总数：{AllCards.Count}");
        }

        // 更新当前显示的卡牌列表（根据选中的分类）

        setCurrentCards();

        int pageStart = currentPageIndex * itemsPerPage;
        int pageEnd = Mathf.Min(pageStart + itemsPerPage, currentCards.Count);
        int targetShowCount = pageEnd - pageStart;          // 当前页应有的卡片数
        int currentShowCount = BagCardsHolder.cardLogics.Count; // UI 中已存在的卡片数
        int needFill = Mathf.Max(0, targetShowCount - currentShowCount);

        if (needFill > 0)
        {
            int startIndex = pageStart + currentShowCount;   // 从 UI 已有位置的下一张开始补
            int endIndex = Mathf.Min(startIndex + needFill, currentCards.Count);
            for (int i = startIndex; i < endIndex; i++)
            {
                Card card = currentCards[i];
                yield return BagCardsHolder.AddCard(card, drawPilePoint);
            }
        }
        GenerateDots();
        yield return null;
        setupBtnPage();

        // 延时解锁，恢复交互
        yield return new WaitForSeconds(LockDuration);
        ResetPageLock();
        Debug.Log("【背包添加卡牌流程全部结束】");
    }


    /// <summary>
    /// 统一切换卡牌分类（核心入口）
    /// </summary>
    private void SwitchCardType(List<Card> targetCards)
    {
        // 更新当前显示卡牌
        updateCurrentCards(targetCards);
        // 重新生成分页圆点
        GenerateDots();
        // 刷新左右翻页按钮位置（跟随圆点整体宽度变化）
        setupBtnPage();

        if (maxPageCount <= 0)
        {
            // 修复：空列表时清空UI
            currentPageIndex = 0;
            UpdatePanelGA emptyPanel = new UpdatePanelGA(0, 0);
            ActionSystem.Instance.Perform(emptyPanel);
            return;
        }

        // 修复：强制重置到第0页，避免旧分页索引越界
        SwitchPage(0);
        // 修复：重新计算endIndex，基于新的currentCards长度
        int newEndIndex = Mathf.Min(itemsPerPage, currentCards.Count);
        UpdatePanelGA firstPage = new UpdatePanelGA(0, newEndIndex);
        ActionSystem.Instance.Perform(firstPage);
    }
    /// <summary>
    /// 局部刷新背包：重读数据 + 刷新当前分类页面（替代 setUP）
    /// </summary>
    public void RefreshCurrentBagView()
    {
        if (_isPageChanging) return;
        // 根据当前选中的分类，刷新视图
        switch (e_NowBagCardType)
        {
            case E_bagCardType.allCard:
                SwitchCardType(AllCards);
                break;
            case E_bagCardType.actCard:
                SwitchCardType(ActCards);
                break;
            case E_bagCardType.eleCard:
                SwitchCardType(EleCards);
                break;
            case E_bagCardType.comCard:
                SwitchCardType(ComCards);
                break;
            case E_bagCardType.weaCard:
                SwitchCardType(WeaCards);
                break;
        }
    }

    /// <summary>
    /// 根据物品更新总数和每页数量
    /// </summary>
    void GeneratePages()
    {
        nowTotalCardCount = currentCards?.Count ?? 0;
        // 修复：避免除以0或负数
        if (itemsPerPage <= 0) itemsPerPage = 16; // 兜底默认值
        maxPageCount = nowTotalCardCount <= 0 ? 0 : Mathf.CeilToInt((float)nowTotalCardCount / itemsPerPage);
        currentPageIndex = Mathf.Clamp(currentPageIndex, 0, maxPageCount - 1); // 限制索引范围
    }


    /// <summary>
    /// 根据页面数量，动态生成对应的圆点Toggle
    /// </summary>
    private void GenerateDots()
    {
        // 先清空旧的圆点
        foreach (var d in dots) Destroy(d.gameObject);
        dots.Clear();

        for (int i = 0; i < maxPageCount; i++)
        {
            Toggle dot = Instantiate(dotPrefab, toggleGroup.transform);
            dot.group = toggleGroup;
            dot.name = $"Dot_{i}";

            // 点击圆点跳转到对应页
            int index = i; // 局部变量捕获
            dot.onValueChanged.AddListener(isOn =>
            {
                // 1. 防抖拦截 + 非选中拦截
                if (!isOn || _isPageChanging) return;

                // 加锁
                _isPageChanging = true;

                int begin = index * itemsPerPage;
                int end = Mathf.Min((index + 1) * itemsPerPage, currentCards.Count);
                UpdatePanelGA updatePanelGA = new UpdatePanelGA(begin, end);
                ActionSystem.Instance.Perform(updatePanelGA);

                // 只更新页码，不再二次触发Toggle事件（切断循环）
                currentPageIndex = index;
            });

            dots.Add(dot);
        }
    }
    /// <summary>
    /// 切换到指定页码,激活dot
    /// </summary>
    void SwitchPage(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= maxPageCount || _isPageChanging || dots.Count == 0)
            return;

        currentPageIndex = pageIndex;
        for (int i = 0; i < dots.Count; i++)
        {
            if (dots[i] == null) continue; // 空值保护
            if (i == currentPageIndex)
                dots[i].isOn = true;
            else
                dots[i].SetIsOnWithoutNotify(false);
        }
    }


    void setupBtnPage()
    {
        btnPre.onClick.AddListener(OnPrePage);
        btnNext.onClick.AddListener(OnNextPage);

        // ===== 关键：强制刷新 ToggleGroup 的布局 =====
        RectTransform toggleGroupRect = toggleGroup.transform as RectTransform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(toggleGroupRect);
        // 现在再取宽度，就能拿到正确值了
        float toggleGroupWidth = toggleGroupRect.rect.width;

        RectTransform btnPreRect = btnPre.transform as RectTransform;
        RectTransform btnNextRect = btnNext.transform as RectTransform;

        float buttonOffset = 30f;
        if (btnPreRect != null)
        {
            float btnPreX = -toggleGroupWidth / 2f - buttonOffset;
            btnPreRect.anchoredPosition = new Vector2(btnPreX, btnPreRect.anchoredPosition.y);
        }
        if (btnNext != null)
        {
            float btnNextX = toggleGroupWidth / 2f + buttonOffset;
            btnNextRect.anchoredPosition = new Vector2(btnNextX, btnNextRect.anchoredPosition.y);
        }
        Debug.Log("ToggleGroup宽度：" + toggleGroupWidth);
        Debug.Log("btnPre位置：" + btnPreRect.anchoredPosition);
    }
    /// <summary>
    /// 上一页
    /// </summary>
    void OnPrePage()
    {
        int pre = currentPageIndex - 1;
        if (pre < 0) pre = maxPageCount - 1;
        SwitchPage(pre);
    }
    /// <summary>
    /// 下一页
    /// </summary>
    void OnNextPage()
    {
        int next = currentPageIndex + 1;
        if (next >= maxPageCount) next = 0;
        SwitchPage(next);
    }

    void ResetPageLock()
    {
        _isPageChanging = false;
        // 恢复所有圆点可点击
        SetAllInteractable(true);
    }

    /// <summary>
    /// 统一控制：圆点 + 左右按钮 是否可点击
    /// </summary>
    void SetAllInteractable(bool enable)
    {
        // 圆点
        foreach (var dot in dots)
        {
            if (dot != null)
                dot.interactable = enable;
        }
        // 左右箭头按钮
        if (btnPre != null) btnPre.interactable = enable;
        if (btnNext != null) btnNext.interactable = enable;
    }

    void setCurrentCards()
    {
        // 根据当前选中的分类，刷新视图
        switch (e_NowBagCardType)
        {
            case E_bagCardType.allCard:
                updateCurrentCards(AllCards);
                break;
            case E_bagCardType.actCard:
                updateCurrentCards(ActCards);
                break;
            case E_bagCardType.eleCard:
                updateCurrentCards(EleCards);
                break;
            case E_bagCardType.weaCard:
                updateCurrentCards(WeaCards);
                break;
        }
    }

    void updateCurrentCards(List<Card> cards)
    {
        if (currentCards==null)
        {
            currentCards = new List<Card>();
        }
        currentCards.Clear();
        currentCards.AddRange(cards);
        GeneratePages();
    }

    void setupBtnCard()
    {
        btnAllCard.onClick.RemoveAllListeners();
        btnActCard.onClick.RemoveAllListeners();
        btnEleCard.onClick.RemoveAllListeners();
        btnComCard.onClick.RemoveAllListeners();

        btnAllCard.onClick.AddListener(() =>
        {
            e_NowBagCardType = E_bagCardType.allCard;
            SwitchCardTypeWithInterrupt(AllCards);
        });
        btnActCard.onClick.AddListener(() => 
        {
            e_NowBagCardType = E_bagCardType.actCard;
            SwitchCardTypeWithInterrupt(ActCards);
        });
        btnEleCard.onClick.AddListener(() => 
        {
            e_NowBagCardType = E_bagCardType.eleCard;
            SwitchCardTypeWithInterrupt(EleCards);
        });
        btnComCard.onClick.AddListener(() => 
        {
            e_NowBagCardType = E_bagCardType.comCard;
            SwitchCardTypeWithInterrupt(ComCards);
        });
        btnWeaCard.onClick.AddListener(() => 
        {
            e_NowBagCardType = E_bagCardType.weaCard;
            SwitchCardTypeWithInterrupt(WeaCards);
        });
    }
    private void SwitchCardTypeWithInterrupt(List<Card> targetCards)
    {
        updateCurrentCards(targetCards);
        GenerateDots();
        setupBtnPage();

        if (maxPageCount <= 0)
        {
            currentPageIndex = 0;
            UpdatePanelGA emptyPanel = new UpdatePanelGA(0, 0);
            ActionSystem.Instance.Schedule(emptyPanel, null, ActionScheduleMode.Interrupt);
            return;
        }

        SwitchPage(0);
        int newEndIndex = Mathf.Min(itemsPerPage, currentCards.Count);
        UpdatePanelGA firstPage = new UpdatePanelGA(0, newEndIndex);
        // 关键：使用中断模式
        ActionSystem.Instance.Schedule(firstPage, null, ActionScheduleMode.Interrupt);
    }
    #region 卡牌工具


    /// <summary>
    /// 销毁一张卡牌（播放移动到弃牌堆的动画，然后销毁）
    /// </summary>
    private IEnumerator DiscardCard(CardLogic cardLogic)
    {
        if (cardLogic == null) yield break;

        // 【第一步】先从手牌系统移除（必须最先做！）
        currentCards.Remove(cardLogic.card);

        BagCardsHolder.RemoveCard(cardLogic.card);


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



    /// <summary>
    /// 清空当前卡牌界面（播放移动到弃牌堆的动画，然后销毁）
    /// </summary>
    public IEnumerator ClearAllCard()
    {
        // 关键：拷贝副本遍历，杜绝遍历中原集合被修改
        List<CardLogic> tempCardList = new List<CardLogic>(BagCardsHolder.cardLogics);

        foreach (var cardLogic in tempCardList)
        {
            if (cardLogic == null) continue;

            BagCardsHolder.RemoveCard(cardLogic.card);

            if (cardLogic.cardVisual != null)
            {
                Destroy(cardLogic.cardVisual.gameObject);
            }
            if (cardLogic.slotGameObject != null)
            {
                Destroy(cardLogic.slotGameObject);
            }
            Destroy(cardLogic.gameObject);

            // 如果需要逐张销毁带间隔动画，可以加延时
             //yield return new WaitForSeconds(0.1f);
        }

        // 最后统一清空原集合
        BagCardsHolder.cardLogics.Clear();
        BagCardsHolder.selectedCardLogic = null;
        BagCardsHolder.hoveredCardLogic = null;

        yield return null;
    }
    #endregion
}
