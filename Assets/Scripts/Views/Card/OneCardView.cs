using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneCardView : MonoBehaviour
{

    [SerializeField] private GameObject slotPrefab;

    [SerializeField] private Transform slotParent;

    [Header("Visual Parent")]
    [SerializeField] private Transform viewParent;

    [SerializeField] private CardLogic selectedCardLogic;

    [SerializeField] private bool tweenCardReturn = true;


    public IEnumerator creatOneCardView(Card card)
    {
        GameObject newSlot=Instantiate(slotPrefab,slotParent);
        CardLogic newCardLogic=CardViewCreator.Instance.CreateCardVisual(card,newSlot.transform,newSlot.transform,viewParent.transform);
        newCardLogic.slotGameObject = newSlot;
        RegisterCard(newCardLogic);
        yield return new WaitForSeconds(0.15f);
    }

    /// <summary>
    /// 给卡牌注册拖拽事件监听
    /// </summary>
    public void RegisterCard(CardLogic cardLogic)
    {
        // 监听：开始拖拽时 → 调用 BeginDrag
        cardLogic.BeginDragEvent.AddListener(BeginDrag);
        // 监听：结束拖拽时 → 调用 EndDrag
        cardLogic.EndDragEvent.AddListener(EndDrag);
    }

    /// <summary>
    /// 当卡牌开始拖拽时触发
    /// 作用：记录当前正在拖拽哪张卡牌
    /// </summary>
    private void BeginDrag(CardLogic cardLogic)
    {
        selectedCardLogic = cardLogic;
    }

    /// <summary>
    /// 当卡牌结束拖拽时触发
    /// 作用：让卡牌平滑归位到武器槽原位
    /// </summary>
    void EndDrag(CardLogic cardLogic)
    {
        // 如果没有选中卡牌，直接返回
        if (selectedCardLogic == null)
            return;

        // 卡牌拖拽结束 → 用DOTween动画回到原位
        // 如果卡牌是选中状态，就往上偏移一点；否则回到(0,0,0)
        selectedCardLogic.transform.DOLocalMove(
            selectedCardLogic.selected ? new Vector3(0, selectedCardLogic.selectionOffset, 0) : Vector3.zero,
            tweenCardReturn ? .15f : 0 // 动画时长：开启缓动则0.15秒，否则瞬间归位
        ).SetEase(Ease.OutBack); // 回弹曲线，更自然

        // 清空当前选中的卡牌
        selectedCardLogic = null;
    }
}
