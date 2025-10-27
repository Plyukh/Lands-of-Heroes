using System;
using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public class CombatController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private HighlightController highlightController;

    public event Action<Creature> OnCombatComplete;

    /// <summary>
    /// Вызывается, когда нужно провести атаку: заранее 
    /// персонаж уже переместился в нужную клетку.
    /// </summary>
    public async void OnCreatureClicked(Creature attacker, Creature target, AttackType selectedType)
    {
        if (attacker == null || target == null || attacker == target)
            return;
        if (!TurnOrderController.Instance.IsCurrentTurn(attacker))
            return;

        // Убираем все подсветки перед атакой
        highlightController.ClearHighlights();

        // Поворачиваемся лицом к цели
        await attacker.Mover.RotateTowardsAsync(target.transform.position);

        // Запускаем анимацию удара и ждём момента «попадания»
        await PlayAttackSequence(attacker, target, selectedType);

        // Оповещаем, что атака завершилась
        OnCombatComplete?.Invoke(attacker);
    }

    private async Task PlayAttackSequence(Creature attacker, Creature target, AttackType type)
    {
        var anim = attacker.Mover.AnimatorController;
        
        // Проверяем, есть ли эффект двойной атаки
        bool hasDoubleAttack = false;
        if (type == AttackType.Ranged)
        {
            hasDoubleAttack = attacker.EffectManager.HasEffectOfType(EffectType.DoubleShot);
        }
        else
        {
            hasDoubleAttack = attacker.EffectManager.HasEffectOfType(EffectType.DoubleBlow);
        }

        // Устанавливаем цель анимации
        anim.SetAttackTarget(target, attacker);

        // Счетчик ударов (нужно дождаться 1 или 2 ударов)
        int expectedHits = hasDoubleAttack ? 2 : 1;
        int hitCount = 0;
        var tcs = new TaskCompletionSource<bool>();

        Action onHit = null;
        if (type == AttackType.Ranged)
        {
            onHit = () =>
            {
                hitCount++;
                if (hitCount >= expectedHits)
                {
                    anim.OnAttackHit -= onHit;
                    tcs.TrySetResult(true);
                }
            };
            anim.OnAttackHit += onHit;
            anim.PlayAttack();
        }
        else
        {
            // Для ближнего боя дальников используем OnAttackHit, так как HandleAttackHitEvent 
            // всегда вызывает OnAttackHit для дальников независимо от типа атаки
            if (attacker.AttackType == AttackType.Ranged)
            {
                onHit = () =>
                {
                    hitCount++;
                    if (hitCount >= expectedHits)
                    {
                        anim.OnAttackHit -= onHit;
                        tcs.TrySetResult(true);
                    }
                };
                anim.OnAttackHit += onHit;
            }
            else
            {
                onHit = () =>
                {
                    hitCount++;
                    if (hitCount >= expectedHits)
                    {
                        anim.OnMeleeAttackHit -= onHit;
                        tcs.TrySetResult(true);
                    }
                };
                anim.OnMeleeAttackHit += onHit;
            }
            anim.PlayMeleeAttack();
        }

        await tcs.Task;
    }
}
