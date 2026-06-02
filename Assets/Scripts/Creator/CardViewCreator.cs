using UnityEngine;

public class CardViewCreator : MonoBehaviour
{
    public static CardViewCreator Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject cardPrefab;        // 带有 Card 脚本的预制体（逻辑物体）
    [SerializeField] private GameObject cardVisualPrefab;  // CardVisual 预制体

    //[Header("Visual Parent")]
    //[SerializeField] private Transform visualParent;       // CardVisual 的父物体（通常是VisualHandler）

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 创建一张完整的卡牌（逻辑 + 视觉），并放到指定槽位下
    /// </summary>
    public CardLogic CreateCardVisual(Card card, Transform slotParent,Transform drawPilePoint, Transform visualParent)
    {
        // 1. 实例化逻辑 Card（放在槽位下）
        GameObject cardObj = Instantiate(cardPrefab, slotParent);
        CardLogic cardLogic = cardObj.GetComponent<CardLogic>();
        cardLogic.Setup(card);   // 需要先在 Card.cs 中添加 public Card card;

        // 2. Card.Start() 会自动实例化 CardVisual（因为 instantiateVisual = true）
        //    但我们需要在 CardVisual 初始化时把图片赋上去,把位置给他
        //    所以需要修改 CardVisual.Initialize() 读取 card.card.cardArt
        // 重要：强制关闭自动视觉实例化（覆盖预制体设置）

        // 2. 手动创建 CardVisual（仅一次）
        Transform parent = visualParent != null ? visualParent : FindObjectOfType<Canvas>().transform;
        GameObject visualObj = Instantiate(cardVisualPrefab, parent);
        CardVisual visual = visualObj.GetComponent<CardVisual>();
        visual.Initialize(cardLogic, drawPilePoint);
        cardLogic.cardVisual = visual;

        return cardLogic;
    }
}