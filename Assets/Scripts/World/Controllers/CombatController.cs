using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public class CombatController : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Менеджер поиска пути и зоны досягаемости")]
    [SerializeField] private PathfindingManager pathfindingManager;
    [Tooltip("Контроллер подсветки клеток")]
    [SerializeField] private HighlightController highlightController;

    /// <summary>
    /// Событие, когда существо завершило атаку и момент попадания отработал.
    /// </summary>
    public event Action<Creature> OnCombatComplete;

    /// <summary>
    /// Вызывается при клике по существу-цели: совершает атаку или подход.
    /// </summary>
    public async void OnCreatureClicked(Creature attacker, Creature target)
    {
        if (attacker == null || target == null || attacker == target)
            return;

        // Блокируем попытки атаковать вне своего хода
        if (!TurnOrderController.Instance.IsCurrentTurn(attacker))
            return;

        var startCell = attacker.Mover.CurrentCell;
        var targetCell = target.Mover.CurrentCell;
        int speed = attacker.GetStat(CreatureStatusType.Speed);
        var moveType = attacker.MovementType;

        // 1) Дальний бой — сразу стреляем
        if (attacker.AttackType == AttackType.Ranged)
        {
            await PlayAttackSequence(attacker, target);
            return;
        }

        // 2) Для ближнего: находим все соседние к цели свободные клетки
        var neighborCells = pathfindingManager
            .GetReachableCells(targetCell, 1, MovementType.Teleport)
            .Where(c => c.IsWalkable)
            .ToList();

        // 3) Если уже в одной из соседних — сразу атакуем
        if (neighborCells.Contains(startCell))
        {
            await PlayAttackSequence(attacker, target);
            return;
        }

        // 4) Ищем из reachable подходящие соседи
        var reachable = pathfindingManager
            .GetReachableCells(startCell, speed, moveType);

        var candidates = neighborCells.Intersect(reachable).ToList();
        if (candidates.Count == 0)
        {
            Debug.Log($"[CombatController] Нельзя подойти к {target.name} для ближней атаки");
            return;
        }

        // 5) Выбираем ближайшую соседнюю клетку
        var attackPos = candidates
            .OrderBy(c => Vector3.Distance(startCell.transform.position, c.transform.position))
            .First();

        // 6) Подсветка зоны хода и выбранной клетки
        highlightController.ClearHighlights();
        highlightController.HighlightReachable(reachable, startCell);
        attackPos.ShowHighlight(true);

        // 7) Двигаемся или телепортируемся на attackPos
        bool moved = false;
        if (moveType == MovementType.Teleport)
        {
            moved = await attacker.Mover.TeleportToCell(attackPos);
        }
        else
        {
            var path = pathfindingManager.FindPath(startCell, attackPos, moveType);
            if (path != null && path.Count > 0)
                moved = await attacker.Mover.MoveAlongPath(path);
        }

        if (!moved)
            return;

        // 8) Обновляем occupants клеток
        startCell.RemoveOccupant(attacker.gameObject);
        attackPos.AddOccupant(attacker.gameObject, CellObjectType.Creature);

        // 9) Обновляем подсветку от новой позиции (опционально)
        var newReachable = pathfindingManager
            .GetReachableCells(attackPos, speed, moveType);
        highlightController.ClearHighlights();
        highlightController.HighlightReachable(newReachable, attackPos);

        // 10) Запускаем сама анимацию атаки
        await PlayAttackSequence(attacker, target);
    }

    /// <summary>
    /// Поворачивает, ждёт момент попадания в анимации и кидает событие.
    /// </summary>
    private async Task PlayAttackSequence(Creature attacker, Creature target)
    {
        var mover = attacker.Mover;
        var anim = mover.AnimatorController;
        var hitTcs = new TaskCompletionSource<bool>();
        bool isRanged = attacker.AttackType == AttackType.Ranged;

        // A) Поворачиваем к цели
        await mover.RotateTowardsAsync(target.transform.position);

        // B) Готовим анимацию
        anim.SetAttackTarget(target, attacker);

        // C) Ждём события «ударил» или «выстрелил»
        Action onHit = null;
        if (isRanged)
        {
            onHit = () =>
            {
                anim.OnAttackHit -= onHit;
                hitTcs.TrySetResult(true);
            };
            anim.OnAttackHit += onHit;
            anim.PlayAttack();
        }
        else
        {
            onHit = () =>
            {
                anim.OnMeleeAttackHit -= onHit;
                hitTcs.TrySetResult(true);
            };
            anim.OnMeleeAttackHit += onHit;
            anim.PlayMeleeAttack();
        }

        await hitTcs.Task;

        OnCombatComplete?.Invoke(attacker);
    }
}
