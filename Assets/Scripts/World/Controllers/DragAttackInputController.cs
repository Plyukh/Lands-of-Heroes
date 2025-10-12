using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DragAttackInputController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask creatureLayerMask;
    [SerializeField] private LayerMask hexLayerMask;
    [SerializeField] private PathfindingManager pathfindingManager;
    [SerializeField] private HighlightController highlightController;
    [SerializeField] private CombatController combatController;

    [Header("Joystick UI")]
    [SerializeField] private RectTransform joystickRoot;
    [SerializeField] private float dragThreshold = 10f;

    private enum State { Idle, TouchDown, Dragging }
    private State currentState = State.Idle;

    private Creature activeAttacker;
    private Creature activeTarget;
    private HexCell startCell;

    private List<HexCell> meleeNeighbors;
    private List<HexCell> reachableFromStart;
    private List<HexCell> candidates;

    private HexCell highlightedCell;
    private Vector2 touchStartPos;

    private void Update()
    {
        if (Input.touchCount == 0)
            return;

        var touch = Input.touches[0];
        switch (touch.phase)
        {
            case TouchPhase.Began:
                OnTouchDown(touch.position);
                break;
            case TouchPhase.Moved:
                OnTouchMove(touch.position);
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                OnTouchUp(touch.position);
                break;
        }
    }

    private void OnTouchDown(Vector2 screenPos)
    {
        // Raycast по существам
        var ray = mainCamera.ScreenPointToRay(screenPos);
        if (!Physics.Raycast(ray, out var hit, 100f, creatureLayerMask))
            return;
        if (!hit.collider.TryGetComponent<Creature>(out var targetCreature))
            return;

        // Берём текущее активное существо
        var attacker = TurnOrderController.Instance.CurrentCreature;
        if (attacker == null || attacker.AttackType != AttackType.Melee)
            return;
        if (attacker == targetCreature)
            return;

        activeAttacker = attacker;
        activeTarget = targetCreature;
        startCell = attacker.Mover.CurrentCell;
        touchStartPos = screenPos;
        currentState = State.TouchDown;

        // Показываем джойстик
        joystickRoot.position = screenPos;
        joystickRoot.gameObject.SetActive(true);

        // Сбрасываем прошлое превью, чтобы осталась активная зона
        highlightController.ResetPreview();

        // Вычисляем кандидатов:
        var targetCell = targetCreature.Mover.CurrentCell;
        meleeNeighbors = pathfindingManager
            .GetReachableCells(targetCell, 1, MovementType.Teleport)
            .Where(c => c.IsWalkable)
            .ToList();

        int speed = attacker.GetStat(CreatureStatusType.Speed);
        var moveType = attacker.MovementType;
        reachableFromStart = pathfindingManager
            .GetReachableCells(startCell, speed, moveType);

        candidates = meleeNeighbors.Intersect(reachableFromStart).ToList();
    }

    private void OnTouchMove(Vector2 screenPos)
    {
        if (currentState != State.TouchDown && currentState != State.Dragging)
            return;

        if (currentState == State.TouchDown &&
            Vector2.Distance(touchStartPos, screenPos) > dragThreshold)
        {
            currentState = State.Dragging;
        }

        if (currentState != State.Dragging)
            return;

        // Двигаем джойстик
        joystickRoot.position = screenPos;

        // Raycast по клеткам
        var ray = mainCamera.ScreenPointToRay(screenPos);
        if (!Physics.Raycast(ray, out var hitHex, 100f, hexLayerMask) ||
            !hitHex.collider.TryGetComponent<HexCell>(out var cell))
        {
            ClearHighlightedCell();
            return;
        }

        if (candidates.Contains(cell))
        {
            if (cell != highlightedCell)
            {
                highlightedCell = cell;
                var path = pathfindingManager.FindPath(startCell, cell, activeAttacker.MovementType);
                highlightController.PreviewPath(path);
            }
        }
        else
        {
            ClearHighlightedCell();
        }
    }

    private void OnTouchUp(Vector2 screenPos)
    {
        // Если драг был в допустимую клетку — выполняем бой
        if (currentState == State.Dragging && highlightedCell != null)
        {
            StartCoroutine(ExecuteDragAttack(highlightedCell));
        }
        else
        {
            // Отмена — возвращаем активную зону
            highlightController.ResetPreview();
        }

        // Скрываем джойстик и сбрасываем состояние
        joystickRoot.gameObject.SetActive(false);
        currentState = State.Idle;
        highlightedCell = null;
        activeAttacker = null;
        activeTarget = null;
    }

    private IEnumerator ExecuteDragAttack(HexCell attackCell)
    {
        // Вызываем единый API: движение + атака
        var task = combatController.ExecuteCombat(
            activeAttacker,
            activeTarget,
            attackCell);

        yield return new WaitUntil(() => task.IsCompleted);
    }

    private void ClearHighlightedCell()
    {
        highlightedCell = null;
        highlightController.ResetPreview();
    }
}
