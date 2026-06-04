using System;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.EventSystems;
using Unity.Collections;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;

public class CardVisual : MonoBehaviour
{
    private bool initalize = false;



    [Header("CardLogic")]
    public CardLogic parentCardLogic;
    private Transform cardTransform;
    private Vector3 rotationDelta;
    private int savedIndex;
    Vector3 movementDelta;
    private Canvas canvas;

    [Header("References")]
    public Transform visualShadow;
    private float shadowOffset = 20;
    private Vector2 shadowDistance;
    private Canvas shadowCanvas;
    [SerializeField] private GameObject priceTagPanel;
    //[SerializeField] private GameObject ManaPanel;
    [SerializeField] private Transform shakeParent;
    [SerializeField] private Transform tiltParent;
    [SerializeField] private GameObject cardDescription;
    [SerializeField] private Image cardImage;
    [SerializeField] private Text txtcardDescription;
    [SerializeField] private Text txtPrice;
    //[SerializeField] private Text txtMana;

    [Header("Follow Parameters")]
    [SerializeField] private float followSpeed = 30;

    [Header("Rotation Parameters")]
    [SerializeField] private float rotationAmount = 20;
    [SerializeField] private float rotationSpeed = 20;
    [SerializeField] private float autoTiltAmount = 30;
    [SerializeField] private float manualTiltAmount = 20;
    [SerializeField] private float tiltSpeed = 20;

    [Header("Scale Parameters")]
    [SerializeField] private bool scaleAnimations = true;
    [SerializeField] private float scaleOnHover = 1.15f;
    [SerializeField] private float scaleOnSelect = 1.25f;
    [SerializeField] private float scaleTransition = .15f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    [Header("Select Parameters")]
    [SerializeField] private float selectPunchAmount = 20;

    [Header("Hober Parameters")]
    [SerializeField] private float hoverPunchAngle = 5;
    [SerializeField] private float hoverTransition = .15f;

    [Header("Swap Parameters")]
    [SerializeField] private bool swapAnimations = true;
    [SerializeField] private float swapRotationAngle = 30;
    [SerializeField] private float swapTransition = .15f;
    [SerializeField] private int swapVibrato = 5;

    [Header("Curve")]
    [SerializeField] private CurveParameters curve;

    private float curveYOffset;
    private float curveRotationOffset;
    private Coroutine pressCoroutine;

    private void Start()
    {
        shadowDistance = visualShadow.localPosition;
    }

    public void UpdateUIForContext(E_CardDisplayContext context=E_CardDisplayContext.InMath)
    {
        if (priceTagPanel != null)
        {
            priceTagPanel.SetActive(context == E_CardDisplayContext.InShop);
        }
        //if (ManaPanel != null)
        //    ManaPanel.SetActive(context == E_CardDisplayContext.InMath);
    }

    public void SetUp(CardLogic target)
    {
        if (target == null) 
        {
            Debug.Log("实例化为空");
            return; 
        }
        //自己加
        if (cardImage != null  && target.Image != null)
        {
            cardImage.sprite = target.Image;
        }
        if (txtcardDescription != null && target.description != null)
        {
            txtcardDescription.text=target.description;
        }
        if (txtPrice != null && target.price != null)
        {
            txtPrice.text=target.price;
        }
        //if (txtMana != null && target.mana != null)
        //{
        //    txtMana.text=target.mana;
        //}

        HideDescription();
    }

    public void HideDescription()
    {
        cardDescription.gameObject.SetActive(false);
    }
    public void ShowDescription()
    {
        cardDescription.gameObject.SetActive(true);
    }

    public void Initialize(CardLogic target, Transform startPos, int index = 0)
    {
        transform.position = startPos.position;
        //Declarations
        parentCardLogic = target;
        cardTransform = target.transform;
        canvas = GetComponent<Canvas>();
        shadowCanvas = visualShadow.GetComponent<Canvas>();

        //Event Listening
        parentCardLogic.PointerEnterEvent.AddListener(PointerEnter);
        parentCardLogic.PointerExitEvent.AddListener(PointerExit);
        parentCardLogic.BeginDragEvent.AddListener(BeginDrag);
        parentCardLogic.EndDragEvent.AddListener(EndDrag);
        parentCardLogic.PointerDownEvent.AddListener(PointerDown);
        parentCardLogic.PointerUpEvent.AddListener(PointerUp);
        parentCardLogic.SelectEvent.AddListener(Select);

        SetUp(target);

        //Initialization
        initalize = true;
    }



    public void UpdateIndex(int length)
    {
        transform.SetSiblingIndex(parentCardLogic.transform.parent.GetSiblingIndex());
    }

    void Update()
    {
        if (!initalize || parentCardLogic == null) return;

        HandPositioning();
        SmoothFollow();
        FollowRotation();
        CardTilt();

    }

    private void HandPositioning()
    {
        curveYOffset = (curve.positioning.Evaluate(parentCardLogic.NormalizedPosition()) * curve.positioningInfluence) * parentCardLogic.SiblingAmount();
        curveYOffset = parentCardLogic.SiblingAmount() < 5 ? 0 : curveYOffset;
        curveRotationOffset = curve.rotation.Evaluate(parentCardLogic.NormalizedPosition());
    }

    private void SmoothFollow()
    {
        Vector3 verticalOffset = (Vector3.up * (parentCardLogic.isDragging ? 0 : curveYOffset));
        transform.position = Vector3.Lerp(transform.position, cardTransform.position + verticalOffset, followSpeed * Time.deltaTime);
    }

    private void FollowRotation()
    {
        Vector3 movement = (transform.position - cardTransform.position);
        movementDelta = Vector3.Lerp(movementDelta, movement, 25 * Time.deltaTime);
        Vector3 movementRotation = (parentCardLogic.isDragging ? movementDelta : movement) * rotationAmount;
        rotationDelta = Vector3.Lerp(rotationDelta, movementRotation, rotationSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.Clamp(rotationDelta.x, -60, 60));
    }

    private void CardTilt()
    {
        savedIndex = parentCardLogic.isDragging ? savedIndex : parentCardLogic.ParentIndex();
        float sine = Mathf.Sin(Time.time + savedIndex) * (parentCardLogic.isHovering ? .2f : 1);
        float cosine = Mathf.Cos(Time.time + savedIndex) * (parentCardLogic.isHovering ? .2f : 1);

        Vector3 offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float tiltX = parentCardLogic.isHovering ? ((offset.y * -1) * manualTiltAmount) : 0;
        float tiltY = parentCardLogic.isHovering ? ((offset.x) * manualTiltAmount) : 0;
        float tiltZ = parentCardLogic.isDragging ? tiltParent.eulerAngles.z : (curveRotationOffset * (curve.rotationInfluence * parentCardLogic.SiblingAmount()));

        float lerpX = Mathf.LerpAngle(tiltParent.eulerAngles.x, tiltX + (sine * autoTiltAmount), tiltSpeed * Time.deltaTime);
        float lerpY = Mathf.LerpAngle(tiltParent.eulerAngles.y, tiltY + (cosine * autoTiltAmount), tiltSpeed * Time.deltaTime);
        float lerpZ = Mathf.LerpAngle(tiltParent.eulerAngles.z, tiltZ, tiltSpeed / 2 * Time.deltaTime);

        tiltParent.eulerAngles = new Vector3(lerpX, lerpY, lerpZ);
    }

    private void Select(CardLogic cardLogic, bool state)
    {
        DOTween.Kill(2, true);
        float dir = state ? 1 : 0;
        shakeParent.DOPunchPosition(shakeParent.up * selectPunchAmount * dir, scaleTransition, 10, 1);
        shakeParent.DOPunchRotation(Vector3.forward * (hoverPunchAngle/2), hoverTransition, 20, 1).SetId(2);

        if(scaleAnimations)
            transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);

    }

    public void Swap(float dir = 1)
    {
        if (!swapAnimations)
            return;

        DOTween.Kill(2, true);
        shakeParent.DOPunchRotation((Vector3.forward * swapRotationAngle) * dir, swapTransition, swapVibrato, 1).SetId(3);
    }

    private void BeginDrag(CardLogic cardLogic)
    {
        if(scaleAnimations)
            transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);

        canvas.overrideSorting = true;
    }

    private void EndDrag(CardLogic cardLogic, Vector2 endScreenPos)
    {
        canvas.overrideSorting = false;
        transform.DOScale(1, scaleTransition).SetEase(scaleEase);
    }

    private void PointerEnter(CardLogic cardLogic)
    {
        if(scaleAnimations)
            transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);

        DOTween.Kill(2, true);
        shakeParent.DOPunchRotation(Vector3.forward * hoverPunchAngle, hoverTransition, 20, 1).SetId(2);
    }

    private void PointerExit(CardLogic cardLogic)
    {
        if (!parentCardLogic.wasDragged)
            transform.DOScale(1, scaleTransition).SetEase(scaleEase);
    }

    private void PointerUp(CardLogic cardLogic, bool longPress)
    {
        if(scaleAnimations)
            transform.DOScale(longPress ? scaleOnHover : scaleOnSelect, scaleTransition).SetEase(scaleEase);
        canvas.overrideSorting = false;

        visualShadow.localPosition = shadowDistance;
        shadowCanvas.overrideSorting = true;
    }

    private void PointerDown(CardLogic cardLogic)
    {
        if(scaleAnimations)
            transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);
            
        visualShadow.localPosition += (-Vector3.up * shadowOffset);
        shadowCanvas.overrideSorting = false;
    }

}
