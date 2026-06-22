using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FailurePanel : BasePanel
{
    public Button btnAgain;
    public Button btnBreak;
    public override void Init()
    {
        btnAgain.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<FailurePanel>();
            UIManager.Instance.ShowPanel<MathPanel>();
        });
        btnBreak.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<FailurePanel>();
            UIManager.Instance.ShowPanel<CampsitePanel>();
        });
    }
}
