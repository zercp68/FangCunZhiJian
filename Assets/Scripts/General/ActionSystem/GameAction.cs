using System.Collections.Generic;

/// <summary>
/// 所有游戏动作的抽象基类。
/// 每个动作可以包含三个阶段的子动作列表，这些子动作会在不同时机被递归执行。
/// </summary>
public abstract class GameAction
{
    /// <summary>动作执行前发生的反应动作列表</summary>
    public List<GameAction> PreReactions { get; private set; } = new();

    /// <summary>动作执行过程中（主逻辑之后）发生的反应动作列表</summary>
    public List<GameAction> PerformReactions { get; private set; } = new();

    /// <summary>动作执行后发生的反应动作列表</summary>
    public List<GameAction> PostReactions { get; private set; } = new();
}