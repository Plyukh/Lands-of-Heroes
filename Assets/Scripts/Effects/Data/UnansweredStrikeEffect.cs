using UnityEngine;

/// <summary>
/// Эффект "Безответный удар" - существо атакует без получения контратаки
/// Логика обрабатывается в CombatController.PerformCounterattack()
/// </summary>
public class UnansweredStrikeEffect : Effect
{
    public override void Apply(Creature target)
    {
        // Логика обрабатывается в CombatController
    }

    public override void Remove(Creature target)
    {
        // Логика обрабатывается в CombatController
    }
}

