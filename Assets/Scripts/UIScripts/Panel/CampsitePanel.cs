using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CampsitePanel : BasePanel
{
    public Button btnStore;
    public Button btnSyn;
    public Button btnWea;
    public Button btnBattle;
    public override void Init()
    {
        btnStore.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<CampsitePanel>();
            UIManager.Instance.ShowPanel<StorePanel>();
        });
        btnSyn.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<CampsitePanel>();
            UIManager.Instance.ShowPanel<SynthesisPanel>();

        });
        btnWea.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<CampsitePanel>();
            UIManager.Instance.ShowPanel<WeaponPanel>();
        });
        btnBattle.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<CampsitePanel>();

        });
    }

}
