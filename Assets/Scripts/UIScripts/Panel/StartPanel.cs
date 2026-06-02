using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartPanel : BasePanel
{
    public Button btnPlayGame;
    public Button btnAuthors;
    public override void Init()
    {
        btnPlayGame.onClick.AddListener(() =>
        {
            //隐藏自己
            UIManager.Instance.HidePanel<StartPanel>();
            //打开游戏

        });
        btnAuthors.onClick.AddListener(() =>
        {
            //隐藏自己
            UIManager.Instance.HidePanel<StartPanel>();
            //打开创作者名单
            UIManager.Instance.ShowPanel<AuthorsPanel>();
        });
    }

}
