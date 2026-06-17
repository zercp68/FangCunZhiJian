using UnityEngine;

/// <summary>
/// ×ÆÉÕ²ãÊı£¨»ğ£©
/// </summary>
public class BurnStackEffect : Effect
{
    public int burnStacks;

    public override void Setup(CombatStats combatStats)
    {
        combatStats.burnStacks += burnStacks;
    }

    public override GameAction GetGameAction() => null;
}