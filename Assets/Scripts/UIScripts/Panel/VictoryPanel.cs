using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VictoryPanel : BasePanel
{
    public Button btnNext;
    public Button btnBreak;
    public override void Init()
    {
        btnNext.onClick.AddListener(() =>
        {
            //обр╩╧ь

            UIManager.Instance.HidePanel<VictoryPanel>();
            UIManager.Instance.ShowPanel<MathPanel>();
        });
        btnNext.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<VictoryPanel>();
            UIManager.Instance.ShowPanel<CampsitePanel>();
        });
    }
}
