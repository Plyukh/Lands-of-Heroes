using UnityEngine;

/// <summary>
/// Универсальный эффект для специальных типов ударов (Area Strike, Piercing Strike)
/// Логика атаки обрабатывается в CombatController
/// </summary>
public class StrikeEffect : Effect
{
    public StrikeEffect() : base() { }

    public override void Apply(Creature target)
    {
        // Strike эффекты не требуют Apply/Remove логики
        // Они обрабатываются напрямую в CombatController при атаке
    }

    public override void Remove(Creature target)
    {
        // Strike эффекты не требуют Apply/Remove логики
    }
}
