using UnityEngine;

/// <summary>
/// Эффект двойной атаки (DoubleShot/DoubleBlow)
/// Логика обрабатывается автоматически в CombatController при атаке
/// Эффект проверяется через HasEffectOfType, Apply/Remove не требуются
/// </summary>
public class DoubleAttackEffect : Effect
{
    public DoubleAttackEffect() : base() { }

    public override void Apply(Creature target)
    {
        // Эффект двойной атаки обрабатывается в CombatController
        // Ничего не нужно делать в Apply
    }

    public override void Remove(Creature target)
    {
        // Ничего не нужно делать в Remove
    }
}
