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

    public async void MoveAlongPath(Creature creature, List<HexCell> path)
    {
        if (creature == null || path == null || path.Count == 0)
            return;

        switch (creature.MovementType)
        {
            case MovementType.Teleport:
                await HandleTeleportMovement(creature, path);
                break;

            case MovementType.Ground:
            case MovementType.Flying:
                await HandlePathMovement(creature, path);
                break;
        }
    }

    private async Task HandleTeleportMovement(Creature creature, List<HexCell> path)
    {
        var mover = creature.Mover;
        var startCell = mover.CurrentCell;
        HexCell targetCell = path.Last(); // For teleport, only the destination matters

        // Highlight the single target cell
        highlightController.ClearHighlights();
        highlightController.HighlightTeleportTarget(targetCell);

        // Start animation and wait for it to complete
        bool teleported = await mover.TeleportToCell(targetCell);
        if (!teleported)
            return;

        // Update cell occupants
        startCell.RemoveOccupant(creature.gameObject);
        targetCell.AddOccupant(creature.gameObject, CellObjectType.Creature);

        // Clean up and signal completion
        highlightController.ClearHighlights();
        OnMovementComplete?.Invoke(creature);
    }

    private async Task HandlePathMovement(Creature creature, List<HexCell> path)
    {
        var mover = creature.Mover;
        var startCell = mover.CurrentCell;

        // Temporarily highlight path during movement
        void OnStep(HexCell cell) => cell.ShowHighlight(false);
        mover.OnCellEntered += OnStep;

        bool moved = await mover.MoveAlongPath(path);
        mover.OnCellEntered -= OnStep;

        if (!moved)
            return;

        // Update cell occupants
        var endCell = path.Last();
        startCell.RemoveOccupant(creature.gameObject);
        endCell.AddOccupant(creature.gameObject, CellObjectType.Creature);

        // Clean up and signal completion
        highlightController.ClearHighlights();
        OnMovementComplete?.Invoke(creature);
    }
}
