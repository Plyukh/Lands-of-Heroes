using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickInputController : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI & Core")]
    [SerializeField] private JoystickUI joystickUI;
    [SerializeField] private BattlefieldController battlefieldController;
    [SerializeField] private MovementController movementController;
    [SerializeField] private CombatController combatController;
    [SerializeField] private PathfindingManager pathfindingManager;
    [SerializeField] private HighlightController highlightController;

    [Header("Camera & Masks")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask hexLayerMask;
    [SerializeField] private LayerMask creatureLayerMask;

    private JoystickActionType currentType;
    private Creature attacker;
    private Creature targetCreature;
    private HexCell startCell;

    // Move state
    private HashSet<HexCell> allowedCells = new HashSet<HexCell>();
    private HexCell selectedTargetCell;
    private List<HexCell> selectedPath;

    // Melee state
    private HashSet<HexCell> meleeCells = new HashSet<HexCell>();
    private HexCell defaultMeleeCell;
    private List<HexCell> defaultMeleePath;

    // Flag: melee move in progress
    private bool isMeleeMoving = false;

    private void Awake()
    {
        movementController.OnMovementComplete += HandleMovementComplete;
    }

    private void OnDestroy()
    {
        movementController.OnMovementComplete -= HandleMovementComplete;
    }

    public void OnPointerDown(PointerEventData e)
    {
        Vector2 screenPos = e.position;

        // --- Melee: clicked on enemy creature ---
        if (TryPickCreature(screenPos, out var creature)
            && creature != TurnOrderController.Instance.CurrentCreature)
        {
            attacker = TurnOrderController.Instance.CurrentCreature;
            targetCreature = creature;
            startCell = attacker.Mover.CurrentCell;
            currentType = JoystickActionType.Melee;

            // collect all walkable neighbours of the target
            var neighbors = pathfindingManager
                .GetReachableCells(creature.Mover.CurrentCell, 1, MovementType.Teleport)
                .Where(c => c.IsWalkable)
                .ToList();

            meleeCells = new HashSet<HexCell>(neighbors);

            // choose default melee cell by real path length
            defaultMeleeCell = neighbors
                .OrderBy(c =>
                {
                    var path = pathfindingManager.FindPath(startCell, c, attacker.MovementType);
                    return path?.Count ?? int.MaxValue;
                })
                .FirstOrDefault();

            if (defaultMeleeCell != null)
            {
                defaultMeleePath = pathfindingManager
                    .FindPath(startCell, defaultMeleeCell, attacker.MovementType);

                selectedTargetCell = defaultMeleeCell;
                selectedPath = defaultMeleePath;
                highlightController.HighlightPath(defaultMeleePath);
            }

            ShowJoystickAtCell(targetCreature.Mover.CurrentCell);
            return;
        }

        // --- Move: clicked on a cell ---
        if (TryPickCell(screenPos, out var cell))
        {
            attacker = TurnOrderController.Instance.CurrentCreature;
            if (attacker == null) return;

            startCell = attacker.Mover.CurrentCell;
            currentType = JoystickActionType.Move;

            HighlightMoveZone();

            if (!allowedCells.Contains(cell))
                return;

            selectedPath = pathfindingManager.FindPath(startCell, cell, attacker.MovementType);
            selectedTargetCell = cell;
            allowedCells = new HashSet<HexCell>(selectedPath);

            highlightController.HighlightPath(selectedPath);
            ShowJoystickAtCell(cell);
        }
    }

    public void OnDrag(PointerEventData e)
    {
        if (!joystickUI.gameObject.activeSelf)
            return;

        joystickUI.UpdateDrag(e.position);

        if (currentType != JoystickActionType.Melee)
            return;

        // before reaching boundary – default path
        if (!joystickUI.IsReadyToConfirm)
        {
            if (selectedTargetCell != defaultMeleeCell)
            {
                selectedTargetCell = defaultMeleeCell;
                selectedPath = defaultMeleePath;
                highlightController.HighlightPath(defaultMeleePath);
            }
            return;
        }

        // at boundary – pick new melee target
        Vector2 knobPos = joystickUI.KnobScreenPosition;
        HexCell newTarget = null;

        if (TryPickCell(knobPos, out var overCell) && meleeCells.Contains(overCell))
        {
            newTarget = overCell;
        }
        else
        {
            var dragDir = joystickUI.KnobPositionNormalized.normalized;
            newTarget = meleeCells
                .OrderBy(c =>
                {
                    Vector2 dir = ((Vector2)c.transform.position
                                   - (Vector2)startCell.transform.position).normalized;
                    return Vector2.Angle(dir, dragDir);
                })
                .FirstOrDefault();
        }

        if (newTarget != null && newTarget != selectedTargetCell)
        {
            selectedTargetCell = newTarget;
            var path = pathfindingManager.FindPath(startCell, newTarget, attacker.MovementType);
            if (path != null && path.Count > 0)
            {
                selectedPath = path;
                highlightController.HighlightPath(path);
            }
        }
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (!joystickUI.gameObject.activeSelf)
            return;

        joystickUI.Hide();

        // cancel action if not dragged to boundary
        if (!joystickUI.IsReadyToConfirm)
        {
            highlightController.ClearHighlights();
            // show movement zone again for both Move and Melee
            HighlightMoveZone();
            ClearInputState();
            return;
        }

        // confirm
        switch (currentType)
        {
            case JoystickActionType.Move:
                highlightController.ClearHighlights();
                movementController.MoveAlongPath(attacker, selectedPath);
                ClearInputState();
                break;

            case JoystickActionType.Ranged:
                battlefieldController.OnCreatureClicked(attacker);
                ClearInputState();
                break;

            case JoystickActionType.Melee:
                highlightController.ClearHighlights();
                isMeleeMoving = true;
                movementController.MoveAlongPath(attacker, selectedPath);
                // attack will trigger on movement complete
                break;
        }
    }

    private void HandleMovementComplete(Creature movedCreature)
    {
        if (isMeleeMoving && movedCreature == attacker)
        {
            isMeleeMoving = false;
            combatController.OnCreatureClicked(attacker, targetCreature);
            ClearInputState();
        }
    }

    private void HighlightMoveZone()
    {
        int speed = attacker.GetStat(CreatureStatusType.Speed);
        var moveType = attacker.MovementType;
        var reachable = pathfindingManager
            .GetReachableCells(startCell, speed, moveType)
            .Where(c => moveType is MovementType.Flying or MovementType.Teleport
                        ? c.IsWalkable
                        : true)
            .ToList();

        allowedCells = new HashSet<HexCell>(reachable);
        highlightController.HighlightReachable(reachable, startCell);
    }

    private void ShowJoystickAtCell(HexCell cell)
    {
        Vector2 screen = mainCamera.WorldToScreenPoint(cell.transform.position);
        joystickUI.Show(screen);
    }

    private void ClearInputState()
    {
        attacker = null;
        targetCreature = null;
        startCell = null;
        currentType = default;

        allowedCells.Clear();
        selectedTargetCell = null;
        selectedPath = null;

        meleeCells.Clear();
        defaultMeleeCell = null;
        defaultMeleePath = null;

        isMeleeMoving = false;
    }

    private bool TryPickCreature(Vector2 pos, out Creature creature)
    {
        var ray = mainCamera.ScreenPointToRay(pos);
        if (Physics.Raycast(ray, out var hit, 100f, creatureLayerMask)
            && hit.collider.TryGetComponent(out creature))
            return true;

        creature = null;
        return false;
    }

    private bool TryPickCell(Vector2 pos, out HexCell cell)
    {
        var ray = mainCamera.ScreenPointToRay(pos);
        if (Physics.Raycast(ray, out var hit, 100f, hexLayerMask)
            && hit.collider.TryGetComponent(out cell))
            return true;

        cell = null;
        return false;
    }
}
