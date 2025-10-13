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
    public async void OnCreatureClicked(Creature attacker, Creature target)
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
        await PlayAttackSequence(attacker, target);

        // Оповещаем, что атака завершилась
        OnCombatComplete?.Invoke(attacker);
    }

    /// <summary>
    /// Поворачивает (если нужно) и запускает анимацию атаки,
    /// дожидаясь события попадания (OnAttackHit / OnMeleeAttackHit).
    /// </summary>
    private async Task PlayAttackSequence(Creature attacker, Creature target)
    {
        var anim = attacker.Mover.AnimatorController;
        var tcs = new TaskCompletionSource<bool>();

        // Устанавливаем цель анимации
        anim.SetAttackTarget(target, attacker);

        Action onHit = null;
        if (attacker.AttackType == AttackType.Ranged)
        {
            onHit = () =>
            {
                anim.OnAttackHit -= onHit;
                tcs.TrySetResult(true);
            };
            anim.OnAttackHit += onHit;
            anim.PlayAttack();
        }
        else
        {
            onHit = () =>
            {
                anim.OnMeleeAttackHit -= onHit;
                tcs.TrySetResult(true);
            };
            anim.OnMeleeAttackHit += onHit;
            anim.PlayMeleeAttack();
        }

        await tcs.Task;
    }
}
