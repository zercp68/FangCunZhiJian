using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

enum E_bagCardType
{
    allCard,
    actCard,
    eleCard,
    weaCard,
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
    public Button btnWeaCard;

    [Header("设置")]
    public BagCardsHolder BagCardsHolder;
    private List<Card> currentCards;
    private List<Card> AllCards;
    private List<Card> ActCards;
    private List<Card> EleCards;
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
        UpdatePanelGA firstPage = new UpdatePanelGA(0, Mathf.Min(itemsPerPage, currentCards.Count));
        ActionSystem.Instance.Perform(firstPage);
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
        WeaCards = new List<Card>();
        // 3. 安全获取当前卡组副本并赋值（增加空引用检查）
        if (PlayerDataManager.Instance != null)
        {
            List<Card> deckCopy = PlayerDataManager.Instance.GetCurrentDeckCopy();
            AllCards.AddRange(deckCopy); // 赋值核心逻辑（也可直接 deckCards = deckCopy;）
            CardManager.Instance.SplitCardsByType(AllCards, ActCards, EleCards, WeaCards);
            Debug.Log($"MatchSetup: 从PlayerDataManager获取到卡组数量: {deckCopy.Count}"); // 新增日志
        }
        else
        {
            Debug.LogError("PlayerDataManager.Instance 为空，请检查是否挂载该单例！");
        }
    }

    private IEnumerator UpdatePanelPerformer(UpdatePanelGA updatePanelGA)
    {
        // 加载开始：禁用所有圆点
        SetAllInteractable(false);

        //清空当前界面
        yield return ClearAllCard();
        //生成卡牌

        // 修复：增加索引范围校验，避免越界
        int safeBegin = Mathf.Max(0, updatePanelGA.beginIndex);
        int safeEnd = Mathf.Min(updatePanelGA.endIndex, currentCards.Count);
        // 边界防护：如果起始索引大于列表长度，直接退出
        if (safeBegin >= currentCards.Count)
        {
            ResetPageLock();
            yield break;
        }

        for (int i = safeBegin; i < safeEnd; i++)
        {
            // 二次防护：防止并发修改导致的越界
            if (i < 0 || i >= currentCards.Count)
            {
                Debug.LogWarning($"索引{i}越界，currentCards长度：{currentCards.Count}");
                continue;
            }
            Card card = currentCards[i];

            yield return BagCardsHolder.AddCard(card, drawPilePoint);
        }


        Invoke(nameof(ResetPageLock), LockDuration);
        yield break;
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
        // 正在操作中直接拦截
        if (_isPageChanging) yield break;

        _isPageChanging = true;
        SetAllInteractable(false);

        foreach (var cardLogic in cardLogics)
        {
            if (cardLogic == null) continue;

            // 全局卡组移除卡牌
            AllCards.Remove(cardLogic.card);
            // 等待单张卡牌销毁+动画完成
            yield return DiscardCard(cardLogic);
        }

        // 重新按类型拆分卡组
        CardManager.Instance.SplitCardsByType(AllCards, ActCards, EleCards, WeaCards);

        // 弃牌后同样刷新当前页，自动补下一页卡牌进来填满空位

        RefreshCurrentBagView();
        yield return new WaitForSeconds(LockDuration);
        ResetPageLock();

        UpdatePanelGA firstPage = new UpdatePanelGA(0, Mathf.Min(itemsPerPage, currentCards.Count));
        ActionSystem.Instance.AddReaction(firstPage);
    }
    /// <summary> 添加单张卡牌协程 </summary>
    public IEnumerator AddBagCardViewPerformer(List<Card> cards)
    {
        // 防重复执行
        if (_isPageChanging) yield break;

        _isPageChanging = true;
        SetAllInteractable(false);

        // 1. 把卡牌加入总卡组
        AllCards.AddRange(cards);
        // 2. 重新按类型拆分卡牌
        CardManager.Instance.SplitCardsByType(AllCards, ActCards, EleCards, WeaCards);


        // 3. 刷新当前分类页面（关键！重新加载当前页，补全空位）
        RefreshCurrentBagView();

        // 延时解锁，恢复交互
        yield return new WaitForSeconds(LockDuration);
        ResetPageLock();
        UpdatePanelGA firstPage = new UpdatePanelGA(0, Mathf.Min(itemsPerPage, currentCards.Count));
        ActionSystem.Instance.AddReaction(firstPage);
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
        btnWeaCard.onClick.RemoveAllListeners();

        btnAllCard.onClick.AddListener(() =>
        {
            e_NowBagCardType = E_bagCardType.allCard;
            SwitchCardType(AllCards);
        });
        btnActCard.onClick.AddListener(() => 
        {
            e_NowBagCardType = E_bagCardType.actCard;
            SwitchCardType(ActCards);
        });
        btnEleCard.onClick.AddListener(() => 
        {
            e_NowBagCardType = E_bagCardType.eleCard;
            SwitchCardType(EleCards);
        });
        btnWeaCard.onClick.AddListener(() => 
        {
            e_NowBagCardType = E_bagCardType.weaCard;
            SwitchCardType(WeaCards);
        });
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
