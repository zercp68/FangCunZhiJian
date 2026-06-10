
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum E_CardDisplayContext
{
    /// <summary>
    /// 战斗中
    /// </summary>
    InMath,
    /// <summary>
    /// 商店界面
    /// </summary>
    InShop   

}

public class CardLogic : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    //我的
    public Card card {  get; private set; }
    [Header("Card")]
    [SerializeField] private TMP_Text title;

    //[SerializeField] public string mana { get; private set; }
    [SerializeField] public string price { get; private set; }
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
    [Header("Drag Settings")]
    public bool canDrag = true;   // 新增：是否允许拖拽

    [Header("Events")]
    [HideInInspector] public UnityEvent<CardLogic> PointerEnterEvent;
    [HideInInspector] public UnityEvent<CardLogic> PointerExitEvent;
    [HideInInspector] public UnityEvent<CardLogic, bool> PointerUpEvent;
    [HideInInspector] public UnityEvent<CardLogic> PointerDownEvent;
    [HideInInspector] public UnityEvent<CardLogic> BeginDragEvent;
    [HideInInspector] public UnityEvent<CardLogic,Vector2> EndDragEvent;
    [HideInInspector] public UnityEvent<CardLogic, bool> SelectEvent;

    public void Setup(Card card)
    {
        //this.card = card;
        //title.text = card.Title;
        
        //mana= card.ManaCost.ToString();
        price= card.CardMoney.ToString();
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
            //Debug.LogError("场景中没有 Tag 为 'PlayArea' 的物体！请检查设置");
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
        if (!canDrag) return;      // 禁止拖拽
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
        if (!canDrag) return;      // 禁止拖拽

    }

    /// <summary>
    /// 等待帧结束，重置拖拽标记
    /// </summary>
    private IEnumerator FrameWaitResetDragged()
    {
        yield return new WaitForEndOfFrame();
        wasDragged = false;
    }



    public void OnEndDrag(PointerEventData eventData)
    {
        if (!canDrag) return;      // 禁止拖拽
        isDragging = false;
        canvas.GetComponent<GraphicRaycaster>().enabled = true;
        imageComponent.raycastTarget = true;


        //  归位
        EndDragEvent?.Invoke(this, eventData.position);
        StartCoroutine(FrameWaitResetDragged());
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
