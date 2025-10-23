using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class JoystickInputController : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI & Core")]
    [SerializeField] private JoystickUI joystickUI;
    [SerializeField] private BattlefieldController battlefieldController;
    [SerializeField] private MovementController movementController;
    [SerializeField] private CombatController combatController;
    [SerializeField] private PathfindingManager pathfindingManager;
    [SerializeField] private HexGridManager hexGridManager;
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
    // Flag: melee aiming mode for joystick
    private bool isMeleeAimingMode = false;

    private void Awake()
    {
        joystickUI.Initialize();
        movementController.OnMovementComplete += HandleMovementComplete;
    }

    private void OnDestroy()
    {
        movementController.OnMovementComplete -= HandleMovementComplete;
    }

    public void OnPointerDown(PointerEventData e)
    {
        Vector2 screenPos = e.position;
        Vector3 worldPos = e.position;
        int actionCount = 1;

        // --- Melee: clicked on enemy creature ---
        if (TryPickCreature(screenPos, out var creature))
        {
            attacker = TurnOrderController.Instance.CurrentCreature;
            targetCreature = creature;
            startCell = attacker.Mover.CurrentCell;
            AttackType attackType = attacker.AttackType;
            int speed = attacker.GetStat(CreatureStatusType.Speed);

            if (attacker == null) return;

            if (creature == TurnOrderController.Instance.CurrentCreature)
            {
                actionCount = 2;
                joystickUI.SetActionType(JoystickActionType.Defend, 0);
                joystickUI.SetActionType(JoystickActionType.Wait, 1);
            }
            else
            {
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
                if (attackType == AttackType.Ranged)
                {
                    bool isEngagedInMelee = IsEnemyInMeleeRange(attacker);

                    if (attacker.GetStat(CreatureStatusType.Speed) >= pathToTarget.Count - 1)
                    {
                        canReachByMovement = true;
                    }

                    if (!canReachByMovement && !isEngagedInMelee)
                    {
                        joystickUI.SetActionType(JoystickActionType.Ranged, 0);
                    }
                    else if (canReachByMovement)
                    {
                        if (isEngagedInMelee)
                        {
                            joystickUI.SetActionType(JoystickActionType.Melee, 0);
                            joystickUI.SetActionType(JoystickActionType.Melee, 1); // Устанавливаем одинаковые значения для одного действия
                        }
                        else
                        {
                            actionCount = 2;
                            joystickUI.SetActionType(JoystickActionType.Melee, 0);
                            joystickUI.SetActionType(JoystickActionType.Ranged, 1);
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    joystickUI.SetActionType(JoystickActionType.Melee, 0);
                }

                // collect all walkable neighbours of the target
                var neighbors = pathfindingManager
                    .GetReachableCells(creature.Mover.CurrentCell, 1, MovementType.Teleport)
                    .Where(c => c.IsWalkable)
                    .ToList();

                meleeCells = neighbors
                    .Where(c =>
                    {
                        var path = pathfindingManager.FindPath(startCell, c, attacker.MovementType);
                        if (path == null || path.Count - 1 > speed - 1)
                            return false;
                        
                        // Проверяем, что клетка не занята дружественным существом
                        var occupantCreature = c.GetOccupantCreature();
                        if (occupantCreature != null && occupantCreature.Side == attacker.Side)
                            return false;
                            
                        return true;
                    })
                    .ToHashSet();

                // Если с текущей клетки можно атаковать, добавим её в список
                // Но только если это ближний бой и мы находимся рядом с врагом
                if (IsAttackPossibleFrom(startCell, targetCreature))
                {
                    // Для ближнего боя проверяем, что мы находимся рядом с врагом
                    if (attackType == AttackType.Melee)
                    {
                        bool isInMeleeRange = IsInMeleeRange(startCell, targetCreature);
                        if (isInMeleeRange)
                        {
                            meleeCells.Add(startCell);
                        }
                    }
                    else
                    {
                        // Для дальнего боя всегда можно атаковать с места
                        meleeCells.Add(startCell);
                    }
                }

                // If there are no reachable cells to attack from, cancel the action.
                if (meleeCells.Count == 0 && attackType == AttackType.Melee)
                {
                    ClearInputState();
                    return;
                }

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
            }

            worldPos = targetCreature.Mover.CurrentCell.transform.position;
        }
        // --- Move: clicked on a cell ---
        else if (TryPickCell(screenPos, out var cell))
        {
            attacker = TurnOrderController.Instance.CurrentCreature;
            if (attacker == null) return;

            startCell = attacker.Mover.CurrentCell;

            if (attacker.MovementType == MovementType.Ground)
            {

                joystickUI.SetActionType(JoystickActionType.Move, 0);
            }
            else if (attacker.MovementType == MovementType.Flying)
            {

                joystickUI.SetActionType(JoystickActionType.Fly, 0);
            }
            else if (attacker.MovementType == MovementType.Teleport)
            {

                joystickUI.SetActionType(JoystickActionType.Teleport, 0);
            }

            HighlightMoveZone();

            if (!allowedCells.Contains(cell))
                return;

            selectedPath = pathfindingManager.FindPath(startCell, cell, attacker.MovementType);
            selectedTargetCell = cell;
            allowedCells = new HashSet<HexCell>(selectedPath);

            highlightController.HighlightPath(selectedPath);

            worldPos = selectedTargetCell.transform.position;
        }

        screenPos = mainCamera.WorldToScreenPoint(worldPos);
        joystickUI.Show(screenPos, actionCount);
    }

    public void OnDrag(PointerEventData e)
    {
        if (!joystickUI.gameObject.activeSelf)
            return;

        joystickUI.UpdateDrag(e.position);

        currentType = joystickUI.CurrentAction;

        if (currentType == JoystickActionType.Defend || currentType == JoystickActionType.Wait)
        {
            return;
        }
        else if (targetCreature != null && attacker.AttackType == AttackType.Ranged)
        {
            // Переключение режимов прицеливания только если у дальника есть два действия
            if (joystickUI.GetActionCount() == 2)
            {
                bool wantsMelee = joystickUI.CurrentAction == JoystickActionType.Melee;
                bool isAtEdge = joystickUI.IsReadyToConfirm;

                // Вход в режим прицеливания ближней атакой
                if (wantsMelee && isAtEdge && !isMeleeAimingMode)
                {
                    isMeleeAimingMode = true;
                    joystickUI.SetAnimatorForActionCount(1, isMeleeAimingMode); // Анимация на 1 сегмент (зеленый)
                    joystickUI.SetAllSegmentsToAction(JoystickActionType.Melee);
                }
                // Выход из режима прицеливания
                else if (!isAtEdge && isMeleeAimingMode)
                {
                    isMeleeAimingMode = false;
                    joystickUI.SetAnimatorForActionCount(1, isMeleeAimingMode); // Возвращаем анимацию на 2 сегмента
                    // Восстанавливаем исходные типы действий
                    joystickUI.SetActionType(JoystickActionType.Melee, 0);
                    joystickUI.SetActionType(JoystickActionType.Ranged, 1);
                }
            }
        }

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

            // Если выбрана атака с места, путь будет пустым
            if (selectedTargetCell == startCell)
            {
                selectedPath = new List<HexCell>();
                highlightController.ClearHighlights();
                highlightController.HighlightReachable(new List<HexCell> { startCell }, startCell);
            }
            else
            {
                var path = pathfindingManager.FindPath(startCell, newTarget, attacker.MovementType);
                if (path != null && path.Count > 0)
                {
                    selectedPath = path;
                    highlightController.HighlightPath(path);
                }
            }
        }
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (!joystickUI.gameObject.activeSelf)
            return;

        currentType = joystickUI.CurrentAction;

        joystickUI.Hide();

        if (!joystickUI.IsReadyToConfirm)
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
                // Если путь пустой, атакуем сразу. Иначе - сначала идем.
                if (selectedPath == null || selectedPath.Count == 0)
                {
                    AttackType selectedAttackType = currentType == JoystickActionType.Melee
                        ? AttackType.Melee
                        : AttackType.Ranged;
                    combatController.OnCreatureClicked(attacker, targetCreature, selectedAttackType);
                    ClearInputState();
                }
                else
                {
                    isMeleeMoving = true;
                    movementController.MoveAlongPath(attacker, selectedPath);
                    // атака произойдет после завершения движения
                }
                break;

            case JoystickActionType.Defend:
                battlefieldController.OnDefendAction(attacker);
                ClearInputState();
                break;

            case JoystickActionType.Wait:
                battlefieldController.OnWaitAction(attacker);
                ClearInputState();
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
        isMeleeAimingMode = false;
    }

    private bool IsTargetValid(Creature attacker, Creature target)
    {
        if (attacker.Side != target.Side)
        {
            return true;
        }
        return false;
    }

    private bool IsEnemyInMeleeRange(Creature creature)
    {
        HexCell currentCell = creature.Mover.CurrentCell;
        foreach (var neighbor in hexGridManager.GetNeighbors(currentCell))
        {
            var occupantCreature = neighbor.GetOccupantCreature();
            if (occupantCreature != null && occupantCreature.Side != creature.Side)
            {
                return true; // Найден враг в соседней клетке
            }
        }
        return false; // Врагов рядом нет
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

    private bool IsAttackPossibleFrom(HexCell fromCell, Creature targetCreature)
    {
        // Проверяем, может ли атакующий атаковать цель с указанной клетки
        var path = pathfindingManager.FindPath(fromCell, targetCreature.Mover.CurrentCell, attacker.MovementType);
        if (path == null) return false;

        int distance = path.Count - 1;
        int speed = attacker.GetStat(CreatureStatusType.Speed);
        return distance <= speed - 1; // Учитываем, что одна клетка уже занята для атаки
    }

    private bool IsInMeleeRange(HexCell fromCell, Creature targetCreature)
    {
        // Проверяем, что клетки находятся рядом (соседние)
        var neighbors = hexGridManager.GetNeighbors(fromCell);
        return neighbors.Contains(targetCreature.Mover.CurrentCell);
    }
}
