using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class bagSystem : MonoBehaviour
{

    [Header("toggle")]
    public Toggle dotPrefab;
    public List<Toggle> dots;
    public ToggleGroup toggleGroup;
    private int currentPage = 0;

    public Button btnPre;              // 上一页按钮
    public Button btnNext;            // 下一页按钮

    [Header("设置")]
    public Transform BagCardsView;
    private List<Card> currentCards;

    [Header("预制体")]
    public GameObject pagePrefab;      // 单个页面内容的预制体（比如物品格子容器）

    [Header("配置")]
    public int totalItemCount;    // 总共有多少个物品（用来计算需要多少页）
    public int itemsPerPage = 16;      // 每页放多少个Card
    private int pageNum;
    private int currentPageIndex=0;

    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;

    private List<GameObject> pages = new List<GameObject>();

    public void setUP()
    {
        //获取当前卡牌
        setupCards();
        //计算页数
        GeneratePages();
        //给页数，初始化toggle数量
        GenerateDots();
    }



    /// <summary>
    /// 得到当前卡牌
    /// </summary>
    public void setupCards()
    {
        // 1. 初始化deckCards（防止首次使用时为null）
        if (currentCards == null)
        {
            currentCards = new List<Card>();
        }
        else
        {
            // 2. 清空旧数据（避免叠加）
            currentCards.Clear();
        }
        // 3. 安全获取当前卡组副本并赋值（增加空引用检查）
        if (PlayerDataManager.Instance != null)
        {
            List<Card> deckCopy = PlayerDataManager.Instance.GetCurrentDeckCopy();
            currentCards.AddRange(deckCopy); // 赋值核心逻辑（也可直接 deckCards = deckCopy;）
            Debug.Log($"MatchSetup: 从PlayerDataManager获取到卡组数量: {deckCopy.Count}"); // 新增日志
        }
        else
        {
            Debug.LogError("PlayerDataManager.Instance 为空，请检查是否挂载该单例！");
        }
        totalItemCount = currentCards.Count;
    }

    /// <summary>
    /// 根据物品总数和每页数量，动态生成所有页面
    /// </summary>
    void GeneratePages()
    {
        // 先清空旧的页面
        foreach (var p in pages) Destroy(p);
        pages.Clear();

        // 计算需要多少页
        int pageCount = Mathf.CeilToInt((float)totalItemCount / itemsPerPage);

        for (int i = 0; i < pageCount; i++)
        {
            // 实例化一个页面预制体，父物体是BagHolder
            GameObject page = Instantiate(pagePrefab, BagCardsView);
            page.name = $"Page_{i}";
            page.SetActive(false); // 初始隐藏

            // 这里可以给页面传数据，比如加载第 i 页的物品
            // LoadPageData(page, i);

            pages.Add(page);
        }
    }


    /// <summary>
    /// 根据页面数量，动态生成对应的圆点Toggle
    /// </summary>
    private void GenerateDots()
    {
        // 先清空旧的圆点
        foreach (var d in dots) Destroy(d.gameObject);
        dots.Clear();

        for (int i = 0; i < pages.Count; i++)
        {
            Toggle dot = Instantiate(dotPrefab, toggleGroup.transform);
            dot.group = toggleGroup;
            dot.name = $"Dot_{i}";

            // 点击圆点跳转到对应页
            int index = i;
            dot.onValueChanged.AddListener(isOn =>
            {
                //if (isOn) SwitchPage(index);
                //写跳转的逻辑
            });

            dots.Add(dot);
        }
    }
    /// <summary>
    /// 切换到指定页码
    /// </summary>
    void SwitchPage(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= pages.Count) return;

        currentPageIndex = pageIndex;

        // 隐藏所有页面，显示当前页
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].SetActive(i == currentPageIndex);
        }

        // 同步圆点选中状态
        for (int i = 0; i < dots.Count; i++)
        {
            dots[i].SetIsOnWithoutNotify(i == currentPageIndex);
        }
    }
    /// <summary>
    /// 上一页
    /// </summary>
    void OnPrePage()
    {
        int next = currentPageIndex - 1;
        if (next < 0) next = pages.Count - 1; // 循环翻页，不需要可以删掉这行
        SwitchPage(next);
    }

    /// <summary>
    /// 下一页
    /// </summary>
    void OnNextPage()
    {
        int next = currentPageIndex + 1;
        if (next >= pages.Count) next = 0; // 循环翻页，不需要可以删掉这行
        SwitchPage(next);
    }


    #region 卡牌工具
    /// <summary>
    /// 抽取单张牌
    /// </summary>
    /// <returns></returns>
   /* private IEnumerator DrawCard()
    {
        //从牌堆中随机抽一张牌（拓展方法）
        Card card = currentCards.Draw();
        // 调用Holder.addCard方法就行        //创建卡牌UI(从牌堆位置生成)
        yield return pagePrefab.AddCard(card, drawPilePoint);
    }

    /// <summary>
    /// 弃掉一张卡牌（播放移动到弃牌堆的动画，然后销毁）
    /// </summary>
    private IEnumerator DiscardCard(CardLogic cardLogic)
    {
        if (cardLogic == null) yield break;

        // 【第一步】先从手牌系统移除（必须最先做！）
        currentCards.Remove(cardLogic.card);

        bagCardsHolder.RemoveCard(cardLogic.card);


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
    }*/
    #endregion
}
