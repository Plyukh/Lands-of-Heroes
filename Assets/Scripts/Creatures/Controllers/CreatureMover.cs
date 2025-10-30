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

    // Множитель скорости из GameSpeedSettings (1x, 2x или 3x)
    private int speedMultiplier = 1;

    public CreatureAnimatorController AnimatorController => animatorController;
    public HexCell CurrentCell => currentCell;
    public event Action<HexCell> OnCellEntered;

    // ========== Unity Callbacks ==========
    private void Start()
    {
        SubscribeToSpeedSettings();
    }

    private void OnDestroy()
    {
        UnsubscribeFromSpeedSettings();
    }

    // ========== Настройки скорости ==========
    private void SubscribeToSpeedSettings()
    {
        if (GameSpeedSettings.Instance == null)
            return;
        
        // Применяем текущую скорость
        speedMultiplier = GameSpeedSettings.Instance.SpeedMultiplier;
        
        // Подписываемся на изменения
        GameSpeedSettings.Instance.OnSpeedMultiplierChanged += UpdateSpeedMultiplier;
    }

    private void UnsubscribeFromSpeedSettings()
    {
        if (GameSpeedSettings.Instance != null)
            GameSpeedSettings.Instance.OnSpeedMultiplierChanged -= UpdateSpeedMultiplier;
    }

    /// <summary>
    /// Обновляет множитель скорости передвижения и анимаций.
    /// Вызывается автоматически при изменении GameSpeedSettings.
    /// </summary>
    public void UpdateSpeedMultiplier(int newMultiplier)
    {
        speedMultiplier = Mathf.Clamp(newMultiplier, 1, 3);
        
        // Применяем к анимациям
        if (animatorController != null)
            animatorController.SetAnimationSpeed(speedMultiplier);
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
        float distance = Vector3.Distance(startPos, destination);

        // Если уже на месте
        if (distance <= stopThreshold)
        {
            transform.position = destination;
            tcs.TrySetResult(true);
            yield break;
        }

        // Применяем множитель скорости (1x, 2x или 3x)
        float actualSpeed = moveSpeed * speedMultiplier;
        float duration = distance / actualSpeed;
        float elapsed = 0f;

        // Плавное перемещение
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, destination, progress);
            yield return null;
        }

        // Точно устанавливаем финальную позицию
        transform.position = destination;
        tcs.TrySetResult(true);
    }

    public async Task RotateTowardsAsync(Vector3 point)
    {
        Vector3 direction = (point - transform.position).normalized;
        
        // Если направление слишком короткое - не поворачиваемся
        if (direction.sqrMagnitude < 0.0001f)
            return;

        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        float totalAngle = Quaternion.Angle(startRotation, targetRotation);
        
        // Применяем множитель скорости (1x, 2x или 3x)
        float actualRotationSpeed = rotationSpeed * speedMultiplier;
        float duration = totalAngle / actualRotationSpeed;
        float elapsed = 0f;

        // Плавный поворот
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);
            await Task.Yield();
        }

        // Точно устанавливаем финальный поворот
        transform.rotation = targetRotation;
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