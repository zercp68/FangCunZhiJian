using UnityEngine;

/// <summary>
/// ·´µ¯ÉËº¦Ð§¹û
/// </summary>
public class ReflectDamageEffect : Effect
{
    public float reflectPercent;  // 0.2 = 20%

    public override void Setup(CombatStats combatStats) { }
    public override GameAction GetGameAction() => null;
}