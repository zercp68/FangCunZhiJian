using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 在 UpdatePanelGA 类中重写 OnCancel
public class UpdatePanelGA : GameAction
{
    public int beginIndex;
    public int endIndex;
    
    public override void OnCancel()
    {
        // 立即清理背包视图（不播放动画）
        bagSystem.Instance?.CancelCurrentUpdate();
    }
    public UpdatePanelGA(int beginIndex, int endIndex)
    {
        this.beginIndex = beginIndex;
        this.endIndex = endIndex;
    }
}

