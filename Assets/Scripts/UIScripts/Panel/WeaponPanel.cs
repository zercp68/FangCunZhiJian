using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPanel : BasePanel
{
    public Button btnUnEquipLeftWea;
    public Button btnUnEquipRightWea;

    public Button btnChangeLeftWea;
    public Button btnChangeRightWea;
    public Button btnSellWeaCard;

    public Button btnBack;
    public Text txtCurrentMoney;

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

    public override void Init()
    {
        // 确保订阅（防止 OnEnable 时 PlayerDataManager 还未准备好）
        if (PlayerDataManager.Instance != null)
            PlayerDataManager.Instance.OnMoneyChanged += UpdateMoneyDisplay;
        WeaponPanelSystem.Instance.setup();
        InitButtonEvent();
        InitWeaponView();

        // 显示当前金币
        UpdateMoneyDisplay(PlayerDataManager.Instance?.Money ?? 0);
    }
    private void UpdateMoneyDisplay(int money)
    {
        if (txtCurrentMoney != null)
        {
            txtCurrentMoney.text = $"{money}";
        }
    }

    /// <summary> 绑定按钮点击事件 </summary>
    private void InitButtonEvent()
    {
        // 先移除所有监听，避免面板重复打开导致多次触发
        btnUnEquipLeftWea.onClick.RemoveAllListeners();
        btnUnEquipRightWea.onClick.RemoveAllListeners();
        btnChangeLeftWea.onClick.RemoveAllListeners();
        btnChangeRightWea.onClick.RemoveAllListeners();
        // 卸下左手武器
        btnUnEquipLeftWea.onClick.AddListener(() =>
        {
            UnEquipWeaponGA ga = new UnEquipWeaponGA(E_WeaponHand.Left);
            ActionSystem.Instance.Perform(ga);
        });

        // 卸下右手武器
        btnUnEquipRightWea.onClick.AddListener(() =>
        {
            UnEquipWeaponGA ga = new UnEquipWeaponGA(E_WeaponHand.Right);
            ActionSystem.Instance.Perform(ga);
        });

        // 更换为左手武器
        btnChangeLeftWea.onClick.AddListener(() =>
        {
            ChangeWeaGA ga = new ChangeWeaGA(E_WeaponHand.Left);
            ActionSystem.Instance.Perform(ga);
        });

        // 更换为右手武器
        btnChangeRightWea.onClick.AddListener(() =>
        {
            ChangeWeaGA ga = new ChangeWeaGA(E_WeaponHand.Right);
            ActionSystem.Instance.Perform(ga);
        });
        btnSellWeaCard.onClick.AddListener(() =>
        {
            SellWeaponCardsGA sellWeaponCardsGA = new SellWeaponCardsGA();
            ActionSystem.Instance.Perform(sellWeaponCardsGA);
        });
        //返回界面
        btnBack.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<WeaponPanel>();
            UIManager.Instance.ShowPanel<CampsitePanel>();
        });
    }
    /// <summary> 初始化武器视图 & 武器背包 </summary>
    private void InitWeaponView()
    {
        // 执行绘制武器背包
        DrawWeaBagGA drawGa = new DrawWeaBagGA();
        ActionSystem.Instance.Perform(drawGa);

        // 左右武器视图（使用ActionSystem调度协程，和项目规范统一）
        // 如果你的 ActionSystem 支持直接执行协程GA，就用下面方式；
        // 若只需要简单启动，也可以保留StartCoroutine（下方有备选）
        StartCoroutine(WeaponPanelSystem.Instance.UpdateWeaView(E_WeaponHand.Left));
        StartCoroutine(WeaponPanelSystem.Instance.UpdateWeaView(E_WeaponHand.Right));
    }
}
