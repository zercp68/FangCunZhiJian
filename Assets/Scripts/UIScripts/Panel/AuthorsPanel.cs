using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class AuthorsPanel : BasePanel
{
    public Button btnBack;

    public override void Init()
    {
        btnBack.onClick.AddListener(() =>
        {
            //隐藏自己
            UIManager.Instance.HidePanel<AuthorsPanel>();
            //返回开始界面
            UIManager.Instance.ShowPanel<StartPanel>();
        });
    }
}
