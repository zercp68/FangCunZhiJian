
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardLogic : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    //我的
    public Card card {  get; private set; }
    [Header("Card")]
    [SerializeField] private TMP_Text title;

    [SerializeField] private TMP_Text mana;
    [SerializeField] public Sprite Image { get; private set; }
    [SerializeField] public string description { get; private set; }


    private Canvas canvas;
    private Image imageComponent;
    [SerializeField] private bool instantiateVisual = false;
    private Vector3 offset;

    [Header("CardSlot")]
    public GameObject slotGameObject;

    [Header("Movement")]
    [SerializeField] private float moveSpeedLimit = 50;

    [Header("Selection")]
    public bool selected;
    public float selectionOffset = 50;
    private float pointerDownTime;
    private float pointerUpTime;

    [Header("Visual")]
    [SerializeField] private GameObject cardVisualPrefab;
    [HideInInspector] public CardVisual cardVisual;

    [Header("States")]
    public bool isHovering;
    public bool isDragging;
    [HideInInspector] public bool wasDragged;

    [Header("Events")]
    [HideInInspector] public UnityEvent<CardLogic> PointerEnterEvent;
    [HideInInspector] public UnityEvent<CardLogic> PointerExitEvent;
    [HideInInspector] public UnityEvent<CardLogic, bool> PointerUpEvent;
    [HideInInspector] public UnityEvent<CardLogic> PointerDownEvent;
    [HideInInspector] public UnityEvent<CardLogic> BeginDragEvent;
    [HideInInspector] public UnityEvent<CardLogic> EndDragEvent;
    [HideInInspector] public UnityEvent<CardLogic, bool> SelectEvent;

    public void Setup(Card card)
    {
        //this.card = card;
        //title.text = card.Title;
        
        //mana.text = card.Mana.ToString();
        this.card = card;
        Image = card.Image;
        description = card.Description;
        return;

    }
    //1
    private RectTransform playAreaRect;
    private GameObject playAreaObj;
    private void Awake()
    {
        playAreaObj = GameObject.FindGameObjectWithTag("PlayArea");
    }

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        imageComponent = GetComponent<Image>();
        if (playAreaObj != null)
        {
            playAreaRect = playAreaObj.GetComponent<RectTransform>();
            if (playAreaRect == null)
            {
                Debug.LogWarning("出牌区域引用为空！");
            }
        }
        else
        {
            Debug.LogError("场景中没有 Tag 为 'PlayArea' 的物体！请检查设置");
        }
        // 如果已经通过外部创建了视觉，就不再创建
        if (cardVisual != null) return;
        if (!instantiateVisual)
            return;

        //visualHandler = FindObjectOfType<VisualCardsHandler>();
        //cardVisual = Instantiate(cardVisualPrefab, visualHandler ? visualHandler.transform : canvas.transform).GetComponent<CardVisual>();
        //cardVisual.Initialize(this);

    }

    void Update()
    {
        ClampPosition();

        if (isDragging)
        {
            Vector2 targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) - offset;
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            Vector2 velocity = direction * Mathf.Min(moveSpeedLimit, Vector2.Distance(transform.position, targetPosition) / Time.deltaTime);
            transform.Translate(velocity * Time.deltaTime);
        }
    }

    void ClampPosition()
    {
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x, screenBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y, screenBounds.y);
        transform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        BeginDragEvent.Invoke(this);
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = mousePosition - (Vector2)transform.position;
        isDragging = true;
        canvas.GetComponent<GraphicRaycaster>().enabled = false;
        imageComponent.raycastTarget = false;

        wasDragged = true;
    }

    public void OnDrag(PointerEventData eventData)
    {


    }



    /// <summary>
    /// 检测鼠标放开卡牌后的位置，是否在碰撞区域
    /// </summary>
    /// <returns></returns>
    private bool IsOverPlayArea()
    {
        if (playAreaRect == null)
        {
            Debug.LogWarning("出牌区域引用为空！");
            return false;
        }

        RectTransform cardRect = GetComponent<RectTransform>();

        // 1. 把卡牌的世界坐标 → 转换成屏幕坐标（和出牌区的坐标体系对齐）
        Vector2 cardScreenPos = RectTransformUtility.WorldToScreenPoint(null, cardRect.position);

        // 2. 获取出牌区的四个角（世界坐标），再转成屏幕坐标
        Vector3[] areaCorners = new Vector3[4];
        playAreaRect.GetWorldCorners(areaCorners);
        Vector2 areaMin = RectTransformUtility.WorldToScreenPoint(null, areaCorners[0]);
        Vector2 areaMax = RectTransformUtility.WorldToScreenPoint(null, areaCorners[2]);

        // 3. 手动判断卡牌屏幕坐标是否在出牌区的屏幕范围内
        bool isInArea =
            cardScreenPos.x >= areaMin.x &&
            cardScreenPos.x <= areaMax.x &&
            cardScreenPos.y >= areaMin.y &&
            cardScreenPos.y <= areaMax.y;

        Vector2 areaMiddle;
        areaMiddle.x=(areaMin.x + areaMax.x)/2;

        // 仅在出牌区时，分配左右手
        if (isInArea && card != null)
        {
            float areaMiddleX = (areaMin.x + areaMax.x) / 2;
            card.E_WeaponHand = cardScreenPos.x <= areaMiddleX ? E_WeaponHand.Left : E_WeaponHand.Right;
        }


        Debug.Log($"卡牌屏幕坐标: {cardScreenPos} | 出牌区范围: 左下{areaMin}, 右上{areaMax} | 是否在出牌区: {isInArea} | 左边还是右边？：{card.E_WeaponHand}");

        return isInArea;
    }

    /// <summary>
    /// 统一：卡牌归位 + 重置拖拽状态
    /// </summary>
    private void ResetCardToSlot()
    {
        EndDragEvent.Invoke(this);
        StartCoroutine(FrameWaitResetDragged());
    }

    /// <summary>
    /// 等待帧结束，重置拖拽标记
    /// </summary>
    private IEnumerator FrameWaitResetDragged()
    {
        yield return new WaitForEndOfFrame();
        wasDragged = false;
    }

    /// <summary>
    /// 武器牌规则：未装备就装备；已装备就回槽
    /// </summary>
    private void HandleWeaponCard(WeaponCard weaponCard) 
    {
        if (weaponCard.E_WeaponHand == E_WeaponHand.None)
        {
            Debug.Log("武器未指定左右手，回槽");
            ResetCardToSlot();
            return;
        }
        //未装备才装备，已经装备就回槽
        if (!weaponCard.IsWeaponEquipped) 
        {
            Debug.Log($"装备武器：{weaponCard.CardName} 到 {weaponCard.E_WeaponHand}手");

            SetWeaponGA setWeaponGA=new SetWeaponGA(weaponCard);
            ActionSystem.Instance.Perform(setWeaponGA);
        }
        else
        {
            Debug.Log($"武器{weaponCard.CardName}已装备，回槽");
        }

        ResetCardToSlot();
    }

    /// <summary>
    /// 动作牌规则：先匹配武器  够蓝再出牌
    /// </summary>
    private void HandleActionCard(ActionCard actionCard)
    {
        if (!WeaponSystem.Instance.IsMatchWeapon(actionCard))
        {
            Debug.Log($"动作牌{actionCard.CardName}与武器不匹配，回槽");
            ResetCardToSlot();
            return;
        }
        // 2. 检查法力值
        if (!ManaSystem.Instance.HasEnoughMana(actionCard.ManaCost))
        {
            Debug.Log($"动作牌{actionCard.CardName}法力不足，回槽");
            ResetCardToSlot();
            return;
        }
        // 3. 出牌
        Debug.Log($"打出动作牌：{actionCard.CardName}");
        PlayCardGA playCardGA = new PlayCardGA(actionCard);
        ActionSystem.Instance.Perform(playCardGA);
    }

    /// <summary>
    /// 元素牌规则：无需匹配武器  够蓝就出牌
    /// </summary>
    private void HandleElementCard(ElementCard elementCard)
    {
        // 检查法力值
        if (!ManaSystem.Instance.HasEnoughMana(elementCard.ManaCost))
        {
            Debug.Log($"元素牌{elementCard.CardName}法力不足，回槽");
            ResetCardToSlot();
            return;
        }

        // 出牌
        Debug.Log($"打出元素牌：{elementCard.CardName}");
        PlayCardGA playCardGA = new PlayCardGA(elementCard);
        ActionSystem.Instance.Perform(playCardGA);
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        isDragging = false;
        canvas.GetComponent<GraphicRaycaster>().enabled = true;
        imageComponent.raycastTarget = true;

        // ===== 新增：出牌区域检测 =====
        // 法力值足够,直接调用新的IsOverPlayArea
        //只调用1次，缓存结果（消除重复调用）
        bool isInPlayArea = IsOverPlayArea();

        // 不在出牌区 → 直接归位
        if (!isInPlayArea)
        {
            Debug.Log("未在出牌区域，卡牌回槽");
            ResetCardToSlot();
            return;
        }

        if (card is WeaponCard weaponCard)
        {
            HandleWeaponCard(weaponCard);
            return;
        }

        if (card is ActionCard actionCard)
        {
            HandleActionCard(actionCard);
            return;
        }

        if (card is ElementCard elementCard)
        {
            HandleElementCard(elementCard);
            return;
        }

        // 普通卡 → 归位
        ResetCardToSlot();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnterEvent.Invoke(this);
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExitEvent.Invoke(this);
        isHovering = false;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        PointerDownEvent.Invoke(this);
        pointerDownTime = Time.time;

            cardVisual.ShowDescription();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        if (this == null || cardVisual == null)
            return;

        // ======================
        
        cardVisual.HideDescription();
        // ======================

        pointerUpTime = Time.time;

        PointerUpEvent.Invoke(this, pointerUpTime - pointerDownTime > .2f);

        if (pointerUpTime - pointerDownTime > .2f)
            return;

        if (wasDragged)
            return;

        selected = !selected;
        SelectEvent.Invoke(this, selected);

        if (selected)
            transform.localPosition += (cardVisual.transform.up * selectionOffset);
        else
            transform.localPosition = Vector3.zero;
    }



    public void Deselect()
    {
        if (selected)
        {
            selected = false;
            if (selected)
                transform.localPosition += (cardVisual.transform.up * 50);
            else
                transform.localPosition = Vector3.zero;
        }
    }


    public int SiblingAmount()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.parent.childCount - 1 : 0;
    }

    public int ParentIndex()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.GetSiblingIndex() : 0;
    }

    public float NormalizedPosition()
    {
        return transform.parent.CompareTag("Slot") ? ExtensionMethods.Remap((float)ParentIndex(), 0, (float)(transform.parent.parent.childCount - 1), 0, 1) : 0;
    }
}
