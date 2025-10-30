// CreatureMover.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Creature))]
public class CreatureMover : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private CreatureAnimatorController animatorController;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopThreshold = 0.01f;
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Current Cell (assign in inspector or on spawn)")]
    [SerializeField] private HexCell currentCell;

    // Множитель скорости из настроек
    private int currentSpeedMultiplier = 1;

    public CreatureAnimatorController AnimatorController => animatorController;
    public HexCell CurrentCell => currentCell;
    public event Action<HexCell> OnCellEntered;

    private void Start()
    {
        // Подписываемся на изменение настроек скорости
        if (GameSpeedSettings.Instance != null)
        {
            currentSpeedMultiplier = GameSpeedSettings.Instance.SpeedMultiplier;
            GameSpeedSettings.Instance.OnSpeedMultiplierChanged += UpdateSpeedMultiplier;
        }
    }

    private void OnDestroy()
    {
        // Отписываемся при уничтожении
        if (GameSpeedSettings.Instance != null)
        {
            GameSpeedSettings.Instance.OnSpeedMultiplierChanged -= UpdateSpeedMultiplier;
        }
    }

    /// <summary>
    /// Обновляет множитель скорости для передвижения и анимаций
    /// </summary>
    public void UpdateSpeedMultiplier(int multiplier)
    {
        currentSpeedMultiplier = Mathf.Clamp(multiplier, 1, 3);
        
        // Применяем к анимациям
        if (animatorController != null)
        {
            animatorController.SetAnimationSpeed(currentSpeedMultiplier);
        }
    }

    public void SetCurrentCell(HexCell cell, Quaternion rotation)
    {
        if (cell == null) return;
        currentCell = cell;
        transform.position = cell.transform.position;

        transform.rotation = rotation;

        // Корректируем локальный масштаб, чтобы компенсировать масштаб родительской ячейки
        var parentScale = cell.transform.localScale;
        transform.localScale = new Vector3(
            transform.localScale.x / parentScale.x,
            transform.localScale.y / parentScale.y,
            transform.localScale.z / parentScale.z
        );
    }

    public async Task<bool> MoveAlongPath(IReadOnlyList<HexCell> path)
    {
        if (path == null || path.Count == 0)
            return false;

        animatorController.PlayWalk(true);

        foreach (var cell in path)
        {
            await RotateTowardsAsync(cell.transform.position);
            await MoveToPositionAsync(cell.transform.position);

            currentCell = cell;
            OnCellEntered?.Invoke(cell);
        }

        animatorController.PlayWalk(false);
        return true;
    }

    private Task MoveToPositionAsync(Vector3 destination)
    {
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(MoveCoroutine(destination, tcs));
        return tcs.Task;
    }

    private IEnumerator MoveCoroutine(Vector3 destination, TaskCompletionSource<bool> tcs)
    {
        Vector3 startPos = transform.position;
        float dist = Vector3.Distance(startPos, destination);

        if (dist <= stopThreshold)
        {
            transform.position = destination;
            tcs.TrySetResult(true);
            yield break;
        }

        // Применяем множитель скорости к базовой скорости передвижения
        float effectiveSpeed = moveSpeed * currentSpeedMultiplier;
        float duration = dist / effectiveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, destination, elapsed / duration);
            yield return null;
        }

        transform.position = destination;
        tcs.TrySetResult(true);
    }

    public async Task RotateTowardsAsync(Vector3 point)
    {
        Vector3 dir = (point - transform.position).normalized;
        if (dir.sqrMagnitude < 0.0001f)
            return;

        Quaternion from = transform.rotation;
        Quaternion to = Quaternion.LookRotation(dir, Vector3.up);
        float angle = Quaternion.Angle(from, to);
        
        // Применяем множитель скорости к скорости поворота
        float effectiveRotationSpeed = rotationSpeed * currentSpeedMultiplier;
        float duration = angle / effectiveRotationSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.rotation = Quaternion.Slerp(from, to, t);
            await Task.Yield();
        }

        transform.rotation = to;
    }

    private TaskCompletionSource<bool> teleportTcs;
    private HexCell teleportTarget;

    public Task<bool> TeleportToCell(HexCell targetCell)
    {
        teleportTcs?.TrySetResult(false);
        teleportTarget = targetCell;
        teleportTcs = new TaskCompletionSource<bool>();

        animatorController.PlayStartTeleport();
        return teleportTcs.Task;
    }

    public void OnTeleportMove()
    {
        if (teleportTarget == null) return;

        currentCell = teleportTarget;
        transform.position = teleportTarget.transform.position;
        OnCellEntered?.Invoke(teleportTarget);
    }

    public void OnTeleportEnd()
    {
        // запускаем анимацию «выхода» из телепорта
        animatorController.PlayEndTeleport();

        // завершаем ожидание TeleportToCell
        teleportTcs?.TrySetResult(true);
        teleportTcs = null;
        teleportTarget = null;
    }
}