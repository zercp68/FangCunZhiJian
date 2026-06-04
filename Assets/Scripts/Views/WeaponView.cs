using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponView : MonoBehaviour
{
    private E_CardDisplayContext cardDisplayContext = E_CardDisplayContext.InMath;
    [SerializeField] private GameObject slotPrefab;

    [SerializeField] private Transform slotParent;

    [Header("Visual Parent")]
    [SerializeField] private Transform viewParent;

    [SerializeField] private CardLogic selectedCardLogic;

    [SerializeField] private bool tweenCardReturn = true;




    public IEnumerator creatWeapon(WeaponCard weaponCard)
    {
        GameObject NewSlot = Instantiate(slotPrefab, slotParent);
        // 使用 Creator 创建卡牌
        CardLogic newCardLogic = CardViewCreator.Instance.CreateCardVisual(weaponCard, NewSlot.transform, NewSlot.transform, viewParent,cardDisplayContext);

        // 3. 绑定槽位到 CardLogic（关键！后面弃牌要靠它销毁）
        newCardLogic.slotGameObject = NewSlot;

        RegisterCard(newCardLogic);

        yield return new WaitForSeconds(0.15f);
    }

    public void RegisterCard(CardLogic cardLogic)
    {
        cardLogic.BeginDragEvent.AddListener(BeginDrag);
        cardLogic.EndDragEvent.AddListener(EndDrag);
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

}
