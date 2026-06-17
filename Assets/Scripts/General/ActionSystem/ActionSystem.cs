using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 动作调度策略
/// </summary>
public enum ActionScheduleMode
{
    Queued,     // 排队执行（默认）
    Interrupt,  // 中断当前，清空队列，立即执行新动作
    Reject      // 系统正忙则丢弃
}

/// <summary>
/// 动作系统管理器（单例），负责注册执行器/反应、以及执行动作流程。
/// </summary>
public class ActionSystem : Singleton<ActionSystem>
{
    // 动作队列元素
    private class QueuedAction
    {
        public GameAction Action;
        public Action OnFinished;
    }

    private GameAction currentAction = null;   // 当前正在执行的动作

    private bool isProcessing = false;
    private Coroutine currentCoroutine = null;
    private Queue<QueuedAction> actionQueue = new Queue<QueuedAction>();

    // ---------- 私有字段 ----------
    private bool isPerforming = false;                     // 是否正在执行动作   
    // 公开属性，便于外部判断
    public bool IsBusy => isProcessing;
    public int PendingCount => actionQueue.Count;

    private List<GameAction> currentReactionList = null;   // 当前正在收集反应的列表（由Flow临时赋值）

    // 订阅者字典：Key=动作类型，Value=该类型注册的回调列表（回调接收 GameAction）
    private static Dictionary<Type, List<Action<GameAction>>> preSubs = new();   // 前反应订阅者
    private static Dictionary<Type, List<Action<GameAction>>> postSubs = new();  // 后反应订阅者

    // 执行器字典：Key=动作类型，Value=执行该动作的协程方法（每个类型只有一个执行器）
    private static Dictionary<Type, Func<GameAction, IEnumerator>> performers = new();

    // 用于解决取消订阅时委托无法匹配的问题：保存原始委托 -> 包装委托的映射
    private static Dictionary<Delegate, Action<GameAction>> preWrapperMap = new();
    private static Dictionary<Delegate, Action<GameAction>> postWrapperMap = new();

    // ---------- 公共属性 ----------
    public bool IsPerforming => isPerforming;

    // ---------- 对外方法 ----------
    /// <summary>
    /// 执行一个动作（外部调用入口）。
    /// </summary>
    /// <param name="action">要执行的动作实例</param>
    /// <param name="onFinished">执行完成后的回调（可选）</param>
    public void Perform(GameAction action, Action onFinished = null)
    {
        Schedule(action, onFinished, ActionScheduleMode.Queued);
    }
    /// <summary>
    /// 使用指定调度策略执行动作
    /// </summary>
    public void Schedule(GameAction action, Action onFinished, ActionScheduleMode mode)
    {
        switch (mode)
        {
            case ActionScheduleMode.Queued:
                EnqueueAction(action, onFinished);
                break;
            case ActionScheduleMode.Interrupt:
                InterruptAndExecute(action, onFinished);
                break;
            case ActionScheduleMode.Reject:
                if (!isProcessing)
                    ExecuteNow(action, onFinished);
                else
                    Debug.Log($"[ActionSystem] 系统正忙，Reject 动作: {action.GetType().Name}");
                break;
        }
    }
    // ---------- 私有调度逻辑 ----------
    private void EnqueueAction(GameAction action, Action onFinished)
    {
        actionQueue.Enqueue(new QueuedAction { Action = action, OnFinished = onFinished });
        if (!isProcessing)
            ProcessQueue();
    }

    private void InterruptAndExecute(GameAction action, Action onFinished)
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        actionQueue.Clear();

        // 关键修复：通知被中断的动作立即清理
        if (isProcessing && currentAction != null)
        {
            currentAction.OnCancel();
        }

        isProcessing = false;
        ExecuteNow(action, onFinished);
    }

    private void ExecuteNow(GameAction action, Action onFinished)
    {
        if (isProcessing)
        {
            Debug.LogError("ExecuteNow 被调用时系统正忙，这不应该发生");
            return;
        }
        isProcessing = true;
        currentAction = action;
        currentCoroutine = StartCoroutine(Flow(action, () =>
        {
            isProcessing = false;
            currentCoroutine = null;
            currentAction = null;          // 清空
            onFinished?.Invoke();
            // 继续处理队列中剩余的动作（如果有）
            if (actionQueue.Count > 0)
                ProcessQueue();
        }));
    }

    private void ProcessQueue()
    {
        if (isProcessing) return;
        if (actionQueue.Count == 0) return;
        var next = actionQueue.Dequeue();
        ExecuteNow(next.Action, next.OnFinished);
    }





    /// <summary>
    /// 向当前正在处理的反应列表中添加一个新动作（仅供内部及反应回调使用）。
    /// 注意：只能在 PreReactions / PostReactions 的订阅回调中调用，否则无效。
    /// </summary>
    internal void AddReaction(GameAction action)
    {
        if (currentReactionList != null)
            currentReactionList.Add(action);
        else
            Debug.LogError("AddReaction 被调用了，但当前没有正在收集的反应列表（请检查是否在正确的时机调用）");
    }

    /// <summary>
    /// 为指定动作类型附加执行器（协程形式）。每个动作类型只能有一个执行器。
    /// </summary>
    public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction
    {
        Type type = typeof(T);
        // 包装为统一委托签名：GameAction -> IEnumerator
        IEnumerator Wrapped(GameAction action) => performer((T)action);
        if (performers.ContainsKey(type))
            performers[type] = Wrapped;
        else
            performers.Add(type, Wrapped);
    }

    /// <summary>
    /// 移除指定动作类型的执行器。
    /// </summary>
    public static void DetachPerformer<T>() where T : GameAction
    {
        Type type = typeof(T);
        if (performers.ContainsKey(type))
            performers.Remove(type);
    }

    /// <summary>
    /// 订阅某类动作的反应（前或后）。
    /// </summary>
    /// <param name="reaction">反应回调，参数为具体动作类型</param>
    /// <param name="timing">反应时机（PRE 或 POST）</param>
    public static void SubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        // 包装回调：将具体动作类型转为 GameAction
        Action<GameAction> wrapped = (GameAction action) => reaction((T)action);
        var dict = timing == ReactionTiming.PRE ? preSubs : postSubs;
        var wrapperMap = timing == ReactionTiming.PRE ? preWrapperMap : postWrapperMap;

        Type type = typeof(T);
        if (!dict.ContainsKey(type))
            dict[type] = new List<Action<GameAction>>();

        dict[type].Add(wrapped);
        // 保存原始委托与包装委托的映射，以便取消订阅时能准确移除
        wrapperMap[reaction] = wrapped;
    }

    /// <summary>
    /// 取消订阅某类动作的反应。
    /// </summary>
    public static void UnsubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        var dict = timing == ReactionTiming.PRE ? preSubs : postSubs;
        var wrapperMap = timing == ReactionTiming.PRE ? preWrapperMap : postWrapperMap;
        Type type = typeof(T);

        if (!dict.ContainsKey(type)) return;

        if (wrapperMap.TryGetValue(reaction, out var wrapped))
        {
            dict[type].Remove(wrapped);
            wrapperMap.Remove(reaction);
            // 如果该类型的回调列表变空，可选择性删除键（可选）
            if (dict[type].Count == 0)
                dict.Remove(type);
        }
    }

    // ---------- 私有协程流程 ----------
    /// <summary>
    /// 递归执行一个动作及其所有子反应。
    /// </summary>
    private IEnumerator Flow(GameAction action, Action OnFlowFinished = null)
    {
        // 1. 预反应阶段
        currentReactionList = action.PreReactions;
        // 调用所有预订阅者（它们可能会通过 AddReaction 向 _currentReactionList 添加新动作）
        InvokeSubscribers(action, preSubs);
        yield return ExecuteReactions();

        // 2. 执行阶段（先执行主逻辑，再执行 PerformReactions）
        currentReactionList = action.PerformReactions;
        yield return ExecutePerformer(action);      // 执行主逻辑（协程等待）
        yield return ExecuteReactions();             // 执行 PerformReactions 列表

        // 3. 后反应阶段
        currentReactionList = action.PostReactions;
        InvokeSubscribers(action, postSubs);
        yield return ExecuteReactions();

        // 结束，清空当前列表引用
        currentReactionList = null;
        OnFlowFinished?.Invoke();
    }

    /// <summary>
    /// 调用指定动作类型的所有订阅者（触发反应逻辑）。
    /// </summary>
    private void InvokeSubscribers(GameAction action, Dictionary<Type, List<Action<GameAction>>> subsDict)
    {
        Type type = action.GetType();
        if (subsDict.TryGetValue(type, out var list))
        {
            foreach (var callback in list)
                callback(action);
        }
    }

    /// <summary>
    /// 执行当前 _currentReactionList 中的所有动作（递归）。
    /// </summary>
    private IEnumerator ExecuteReactions()
    {
        if (currentReactionList == null) yield break;
        // 使用临时列表复制，防止在递归过程中修改原集合（虽然当前设计不会，但安全起见）
        var snapshot = new List<GameAction>(currentReactionList);
        foreach (var subAction in snapshot)
        {
            yield return Flow(subAction);
        }
    }

    /// <summary>
    /// 执行动作的主逻辑（执行器）。
    /// </summary>
    private IEnumerator ExecutePerformer(GameAction action)
    {
        Type type = action.GetType();
        if (performers.TryGetValue(type, out var performer))
        {
            yield return performer(action);
        }
        else
        {
            Debug.LogWarning($"动作 {type.Name} 没有注册执行器，请使用 AttachPerformer 注册");
        }
    }
}