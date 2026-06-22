using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorePanel : BasePanel
{

    public Text txtCurrentMoney;
    public Button btnRethrowing;
    public Button btnBuyCard;
    public Button btnBack;

    private void OnEnable()
    {
        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
    }

    private void OnDisable()
    {
        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.OnMoneyChanged -= UpdateMoneyDisplay;
    }

    private void UpdateMoneyDisplay(int money)
    {
        if (txtCurrentMoney != null)
        {
            txtCurrentMoney.text = $"{money}";
        }
    }



    public override void Init()
    {
        // 确保订阅（防止 OnEnable 时 PlayerDataManager 还未准备好）
        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;

        StoreSystem.Instance.setUp();
        DrawStoreCardsGA drawStoreCardsGA = new DrawStoreCardsGA();
        ActionSystem.Instance.Perform(drawStoreCardsGA);



        btnRethrowing.onClick.AddListener(() =>
        {
            RethrowGA rethrowGA = new RethrowGA();
            ActionSystem.Instance.Perform(rethrowGA);
        });
        //返回界面
        btnBack.onClick.AddListener(() => 
        {
            UIManager.Instance.HidePanel<StorePanel>();
            UIManager.Instance.ShowPanel<CampsitePanel>();
        });
        //购买卡牌按钮
        btnBuyCard.onClick.AddListener(() =>
        {
            BuyCardGA buyCardGA = new BuyCardGA();
            ActionSystem.Instance.Perform(buyCardGA);
        });

        // 显示当前金币
        UpdateMoneyDisplay(PlayerDataManager.Instance?.Money ?? 0);
    }
}
