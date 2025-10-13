// MovementController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public class MovementController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private HighlightController highlightController;

    public event Action<Creature> OnMovementComplete;

    /// <summary>
    /// Универсальный метод движения:
    /// - Teleport: подсвечиваем только последнюю клетку, ждём OnTeleportEnd,
    ///   обновляем Occupant, сбрасываем подсветку и кидаем OnMovementComplete.
    /// - Walk/Fly: подсвечиваем весь путь, гасим по OnCellEntered, обновляем Occupant,
    ///   сбрасываем подсветку и кидаем OnMovementComplete.
    /// </summary>
    public async void MoveAlongPath(Creature creature, List<HexCell> path)
    {
        if (creature == null || path == null || path.Count == 0)
            return;

        var mover = creature.Mover;
        var startCell = mover.CurrentCell;
        var moveType = creature.MovementType;

        // --- Teleport ---
        if (moveType == MovementType.Teleport)
        {
            // берём последнюю клетку как цель
            HexCell targetCell = path.Last();

            // подсветка единственной целевой клетки
            highlightController.ClearHighlights();
            highlightController.HighlightTeleportTarget(targetCell);

            // стартуем анимацию и ждём события OnTeleportEnd
            bool ok = await mover.TeleportToCell(targetCell);
            if (!ok)
                return;

            // обновляем Occupant
            startCell.RemoveOccupant(creature.gameObject);
            targetCell.AddOccupant(creature.gameObject, CellObjectType.Creature);

            // сбрасываем подсветку и кидаем окончание
            highlightController.ClearHighlights();
            OnMovementComplete?.Invoke(creature);
            return;
        }

        // --- Walk or Fly ---
        void OnStep(HexCell cell) => cell.ShowHighlight(false);
        mover.OnCellEntered += OnStep;

        bool moved = await mover.MoveAlongPath(path);
        mover.OnCellEntered -= OnStep;

        if (!moved)
            return;

        var endCell = path.Last();
        startCell.RemoveOccupant(creature.gameObject);
        endCell.AddOccupant(creature.gameObject, CellObjectType.Creature);

        highlightController.ClearHighlights();
        OnMovementComplete?.Invoke(creature);
    }
}
