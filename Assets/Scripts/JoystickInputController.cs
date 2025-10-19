using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickInputController : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI & Core")]
    [SerializeField] private List<JoystickUI> joystickVariants;
    [SerializeField] private BattlefieldController battlefieldController;
    [SerializeField] private MovementController movementController;
    [SerializeField] private CombatController combatController;
    [SerializeField] private PathfindingManager pathfindingManager;
    [SerializeField] private HighlightController highlightController;

    [Header("Camera & Masks")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask hexLayerMask;
    [SerializeField] private LayerMask creatureLayerMask;

    private JoystickUI currentJoystick;
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
        foreach (var joystick in joystickVariants)
        {
            joystick.Initialize();
        }
        movementController.OnMovementComplete += HandleMovementComplete;
    }

    private void OnDestroy()
    {
        movementController.OnMovementComplete -= HandleMovementComplete;
    }

    private JoystickUI SelectJoystick(int count)
    {
        foreach (var js in joystickVariants)
            if (js.ActionCount == count)
                return js;

        // fallback
        if (joystickVariants.Count > 0)
            return joystickVariants[0];

        Debug.LogError($"[JoystickInputController] Не задан ни один JoystickUI for count={count}");
        return null;
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
            AttackType attackType = attacker.AttackType;
            int speed = attacker.GetStat(CreatureStatusType.Speed);

            if (attacker == null || attacker == creature) return;

            // Проверяем, можно ли атаковать эту цель
            if (!IsTargetValid(attacker, creature))
            {
                // Если цель невалидна (например, союзник для обычной атаки), ничего не делаем
                return;
            }

            // пробуем найти путь к цели по вашему движению
            var pathToTarget = pathfindingManager.FindPath(startCell, creature.Mover.CurrentCell, attacker.MovementType);

            if (attackType == AttackType.Melee)
            {
                bool reachable = pathToTarget != null && pathToTarget.Count - 1 <= speed;
                if (!reachable)
                {
                    // не выделять существо, перейти к обработке клика по клетке
                    return;
                }
            }

            // проверяем, можем ли «дойти» до существа
            bool canReachByMovement = false;
            if(attackType == AttackType.Ranged)
            {
                if (attacker.GetStat(CreatureStatusType.Speed) >= pathToTarget.Count - 1)
                {
                    canReachByMovement = true;
                }

                if (canReachByMovement)
                {
                    currentJoystick = SelectJoystick(2);
                    currentJoystick.SetActionType(JoystickActionType.Melee, 0);
                    currentJoystick.SetActionType(JoystickActionType.Ranged, 1);
                }
                else
                {
                    currentJoystick = SelectJoystick(1);
                    currentJoystick.SetActionType(JoystickActionType.Ranged, 0);
                }
            }
            else
            {
                currentJoystick = SelectJoystick(1);
                currentJoystick.SetActionType(JoystickActionType.Melee, 0);
            }

            // --- NEW: Filter neighbors by reachability ---

            // collect all walkable neighbours of the target
            var neighbors = pathfindingManager
                .GetReachableCells(creature.Mover.CurrentCell, 1, MovementType.Teleport)
                .Where(c => c.IsWalkable)
                .ToList();

            meleeCells = neighbors
                .Where(c =>
                {
                    var path = pathfindingManager.FindPath(startCell, c, attacker.MovementType);
                    return path != null && path.Count - 1 <= speed - 1;
                })
                .ToHashSet();

            // If there are no reachable cells to attack from, cancel the action.
            if (meleeCells.Count == 0 && attackType == AttackType.Melee)
            {
                ClearInputState();
                return;
            }
            // --- END NEW ---

            // choose default melee cell by real path length from the filtered list
            defaultMeleeCell = meleeCells
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

            Vector3 worldPos = targetCreature.Mover.CurrentCell.transform.position;
            screenPos = mainCamera.WorldToScreenPoint(worldPos);

            currentJoystick.Show(screenPos);
            return;
        }

        // --- Move: clicked on a cell ---
        if (TryPickCell(screenPos, out var cell))
        {
            currentJoystick = SelectJoystick(1);

            attacker = TurnOrderController.Instance.CurrentCreature;
            if (attacker == null) return;

            startCell = attacker.Mover.CurrentCell;

            if(attacker.MovementType == MovementType.Ground)
            {

                currentJoystick.SetActionType(JoystickActionType.Move, 0);
            }
            else if(attacker.MovementType == MovementType.Flying)
            {

                currentJoystick.SetActionType(JoystickActionType.Fly, 0);
            }
            else if (attacker.MovementType == MovementType.Teleport)
            {

                currentJoystick.SetActionType(JoystickActionType.Teleport, 0);
            }

            HighlightMoveZone();

            if (!allowedCells.Contains(cell))
                return;

            selectedPath = pathfindingManager.FindPath(startCell, cell, attacker.MovementType);
            selectedTargetCell = cell;
            allowedCells = new HashSet<HexCell>(selectedPath);

            highlightController.HighlightPath(selectedPath);

            Vector3 worldPos = selectedTargetCell.transform.position;
            screenPos = mainCamera.WorldToScreenPoint(worldPos);

            currentJoystick.Show(screenPos);
        }
    }

    public void OnDrag(PointerEventData e)
    {
        if (!currentJoystick.gameObject.activeSelf)
            return;

        currentJoystick.UpdateDrag(e.position);

        currentType = currentJoystick.CurrentAction;

        if (currentType == JoystickActionType.Melee && targetCreature != null)
        {
            if (!currentJoystick.IsReadyToConfirm)
            {
                if (selectedTargetCell != defaultMeleeCell)
                {
                    selectedTargetCell = defaultMeleeCell;
                    selectedPath = defaultMeleePath;
                    highlightController.HighlightPath(defaultMeleePath);
                }
                return;
            }
        }

       Vector2 knobPos = currentJoystick.KnobScreenPosition;
       HexCell newTarget = null;

        if (TryPickCell(knobPos, out var overCell) && meleeCells.Contains(overCell))
        {
            newTarget = overCell;
        }
        else
        {
            var dragDir = currentJoystick.KnobPositionNormalized.normalized;
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
        if (!currentJoystick.gameObject.activeSelf)
            return;

        currentType = currentJoystick.CurrentAction;

        currentJoystick.Hide();

        if (!currentJoystick.IsReadyToConfirm)
        {
            highlightController.ClearHighlights();
            // show movement zone again for both Move and Melee
            HighlightMoveZone();
            ClearInputState();
            return;
        }
        switch (currentType)
        {
            case JoystickActionType.Move:
            case JoystickActionType.Fly:
            case JoystickActionType.Teleport:
                movementController.MoveAlongPath(attacker, selectedPath);
                ClearInputState();
                break;

            case JoystickActionType.Ranged:
                battlefieldController.OnCreatureClicked(targetCreature);
                ClearInputState();
                break;

            case JoystickActionType.Melee:
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
                        // Конвертация: наш currentType (JoystickActionType) → AttackType
            AttackType selectedAttackType = currentType == JoystickActionType.Melee
                            ? AttackType.Melee
                            : AttackType.Ranged;
            
                        // Передаём выбор игрока в CombatController
            combatController.OnCreatureClicked(attacker, targetCreature, selectedAttackType);
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

    private bool IsTargetValid(Creature attacker, Creature target)
    {
        if(attacker.Side != target.Side)
        {
            return true;
        }
        return false;
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
