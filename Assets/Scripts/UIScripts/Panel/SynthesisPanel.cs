using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SynthesisPanel : BasePanel
{
    public Button btnBack;
    public override void Init()
    {

        //初始化背包系统
        bagSystem.Instance.setUP();
        //绑定按钮事件
        SynthesisSystem.Instance.setup();
        btnBack.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<SynthesisPanel>();
            UIManager.Instance.ShowPanel<CampsitePanel>();
        });
    }



}
