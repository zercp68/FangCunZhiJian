using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MathPanel : BasePanel
{
    private Level currentLevel;

    public override void Init()
    {
        // 1. 获取当前关卡
        currentLevel = LevelManager.Instance?.GetCurrentLevel();
        if (currentLevel == null)
        {
            Debug.LogError("当前关卡数据为空，无法开始战斗！");
            return;
        }

        // 2. 启动战斗初始化（协程，等待内部异步完成）
        StartCoroutine(StartBattle());
    }

    private IEnumerator StartBattle()
    {
        // 调用 MatchSetupSystem 的初始化（如果它也是协程，就 yield 等待）
        yield return MatchSetupSystem.Instance.SetupCoroutine(currentLevel);
        
        // 这里可以做一些进入战斗的 UI 切换动画
        Debug.Log("战斗初始化完成！");
    }
}
