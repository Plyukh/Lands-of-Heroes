using UnityEngine;

/// <summary>
/// Эффект "Взрывной выстрел" - projectile наносит урон по области вокруг целевой клетки
/// Логика обрабатывается в CombatController после попадания снаряда
/// </summary>
public class ExplosiveShotEffect : Effect
{
    public override void Apply(Creature target)
    {
        // Логика обрабатывается в CombatController
        // Этот эффект используется как маркер для проверки HasEffectOfType
    }

    public override void Remove(Creature target)
    {
        // Логика обрабатывается в CombatController
    }
}

