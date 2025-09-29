using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Creature))]
public class CreatureMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("—корость движени€ (единиц Unity в секунду)")]
    [SerializeField] private float moveSpeed = 2f;
    [Tooltip("ѕорог останова (рассто€ние до цели)")]
    [SerializeField] private float stopThreshold = 0.01f;

    [Header("Current Cell (assign manually)")]
    [SerializeField] private HexCell currentCell;
    public HexCell CurrentCell => currentCell;

    private void Reset()
    {
        // ≈сли компонент добавлен во врем€ редактировани€, пытаемс€ найти HexCell в родител€х
        currentCell = GetComponentInParent<HexCell>();
    }

    public void SetCurrentCell(HexCell cell)
    {
        if (cell == null) return;
        currentCell = cell;
        transform.position = cell.transform.position;
    }

    public bool MoveToCellImmediate(HexCell targetCell)
    {
        if (currentCell == null || targetCell == null)
            return false;

        SetCurrentCell(targetCell);
        return true;
    }

    public Task<bool> MoveAlongPath(List<HexCell> path)
    {
        var tcs = new TaskCompletionSource<bool>();
        if (path == null || path.Count == 0)
        {
            tcs.SetResult(false);
            return tcs.Task;
        }

        StartCoroutine(PerformMovement(path, tcs));
        return tcs.Task;
    }

    private IEnumerator PerformMovement(List<HexCell> path, TaskCompletionSource<bool> tcs)
    {
        foreach (var cell in path)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = cell.transform.position;
            float dist = Vector3.Distance(startPos, endPos);
            float duration = dist / moveSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                yield return null;
            }

            // досылаем ровно в центр клетки
            transform.position = endPos;
            currentCell = cell;
        }

        tcs.SetResult(true);
    }

    public Task<bool> MoveToCell(HexCell targetCell)
    {
        return Task.FromResult(MoveToCellImmediate(targetCell));
    }
}
