using UnityEngine;


/// <summary>
/// »ØÑª±ÈÀý£¨Ä¾£©
/// </summary>
public class HealPercentEffect : Effect
{
    public float healPercent;

    public override void Setup(CombatStats combatStats)
    {
        combatStats.healPercent += healPercent;
    }

    public override GameAction GetGameAction() => null;
}