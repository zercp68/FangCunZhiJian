using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SynthesisPanel : BasePanel
{
    //当前卡片链表
    public List<Card> currentCards;
    public override void Init()
    {

        //初始化背包系统
        bagSystem.Instance.setUP();
        //绑定按钮事件
        SynthesisSystem.Instance.setup();
        //初始化第一页
    }



}
