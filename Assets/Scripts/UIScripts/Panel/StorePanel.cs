using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorePanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<CardData> deckData1;
    [SerializeField] private List<CardData> deckData2;
    [SerializeField] private List<CardData> deckData3;
    //    private void Start()
    //    {
    //        CardSystem.Instance.Setup(deckData);
    //    }
    public Button btuRethrowing;

    private void Start()
    {
        StoreSystem.Instance.setUp(deckData1,deckData2,deckData3);
        DrawStoreCardsGA drawStoreCardsGA = new DrawStoreCardsGA();
        ActionSystem.Instance.Perform(drawStoreCardsGA);

        btuRethrowing.onClick.AddListener(() =>
        {
            RethrowGA rethrowGA = new RethrowGA();
            ActionSystem.Instance.Perform(rethrowGA);
        });
    }
}
