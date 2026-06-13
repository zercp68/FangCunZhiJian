using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponView : MonoBehaviour
{
    private E_CardDisplayContext cardDisplayContext = E_CardDisplayContext.InMath;

    [SerializeField] private GameObject slotPrefab; // 卡牌槽位预制体
    [SerializeField] private Transform slotParent;  // 槽位父节点
    [SerializeField] private Transform viewParent;  // 视觉表现父节点
    [SerializeField] private bool tweenCardReturn = true; // 是否启用卡牌归位动画

    private CardLogic selectedCardLogic; // 选中的卡牌逻辑
    private CardLogic weaCardLogic;      // 当前武器视图的卡牌逻辑

    // 封装属性：对外只读，对内可写，加空值校验
    public CardLogic WeaCardLogic
    {
        get => weaCardLogic;
        private set => weaCardLogic = value;
    }


    public IEnumerator creatWeapon(WeaponCard weaponCard)
    {
        // 先销毁已有卡牌（避免重复创建）
        if (WeaCardLogic != null)
        {
            RemoveCard(WeaCardLogic.card);
            Destroy(WeaCardLogic.gameObject);
            WeaCardLogic = null;
        }

        GameObject NewSlot = Instantiate(slotPrefab, slotParent);
        // 使用 Creator 创建卡牌
        WeaCardLogic = CardViewCreator.Instance.CreateCardVisual(weaponCard, NewSlot.transform, NewSlot.transform, viewParent,cardDisplayContext);

        // 3. 绑定槽位到 CardLogic（关键！后面弃牌要靠它销毁）
        WeaCardLogic.slotGameObject = NewSlot;

        RegisterCard(WeaCardLogic);

        yield return new WaitForSeconds(0.15f);
    }

    /// <summary>
    /// 移除卡牌
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public CardLogic RemoveCard(Card card)
    {
        if (WeaCardLogic == null) return null;
        // 添加到 cards 列表并绑定事件
        UnregisterCard(WeaCardLogic);
        return WeaCardLogic;
    }

    public void RegisterCard(CardLogic cardLogic)
    {
        cardLogic.BeginDragEvent.AddListener(BeginDrag);
        cardLogic.EndDragEvent.AddListener(EndDrag);
    }
    public void UnregisterCard(CardLogic cardLogic)
    {

        cardLogic.BeginDragEvent.RemoveListener(BeginDrag);
        cardLogic.EndDragEvent.RemoveListener(EndDrag);
    }
    private void BeginDrag(CardLogic cardLogic)
    {
        selectedCardLogic = cardLogic;
    }


    void EndDrag(CardLogic cardLogic, Vector2 endScreenPos)
    {
        if (selectedCardLogic == null)
            return;

        selectedCardLogic.transform.DOLocalMove(selectedCardLogic.selected ? new Vector3(0, selectedCardLogic.selectionOffset, 0) : Vector3.zero, tweenCardReturn ? .15f : 0).SetEase(Ease.OutBack);


        selectedCardLogic = null;

    }
    // 生命周期：销毁时清理残留事件和动画
    private void OnDestroy()
    {
        if (WeaCardLogic != null)
        {
            UnregisterCard(WeaCardLogic);
        }
        DOTween.Kill(transform); // 停止当前对象的所有DOTween动画
    }
}
