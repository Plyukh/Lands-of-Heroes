using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Creature))]
public class CreatureMover : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] CreatureAnimatorController animatorController;

    [Header("Movement Settings")]
    [Tooltip("�������� �������� (������ Unity � �������)")]
    [SerializeField] private float moveSpeed = 2f;
    [Tooltip("����� �������� (���������� �� ����)")]
    [SerializeField] private float stopThreshold = 0.01f;
    [Tooltip("�������� �������� (�������/�)")]
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Current Cell (assign manually)")]
    [SerializeField] private HexCell currentCell;
    public HexCell CurrentCell => currentCell;

    private TaskCompletionSource<bool> teleportTcs;
    private HexCell teleportTarget;

    public void SetCurrentCell(HexCell cell)
    {
        if (cell == null) return;
        currentCell = cell;
        transform.position = cell.transform.position;
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
        animatorController.PlayWalk(true);

        foreach (var cell in path)
        {
            Vector3 endPos = cell.transform.position;

            // 1) ������ ������������ � ��������� ������
            yield return StartCoroutine(RotateTowardsSmooth(endPos));

            // 2) ����� ��������� �� ������
            Vector3 startPos = transform.position;
            float dist = Vector3.Distance(startPos, endPos);
            float duration = dist / moveSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                yield return null;
            }

            transform.position = endPos;
            currentCell = cell;
        }

        animatorController.PlayWalk(false);
        tcs.SetResult(true);
    }

    private IEnumerator RotateTowardsSmooth(Vector3 targetPos)
    {
        // ��������� ������� ����������
        Vector3 dir = (targetPos - transform.position).normalized;
        if (dir.sqrMagnitude < 0.001f) yield break;

        // ������� �������
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);

        // ���� ����� ������� � ������� ������������
        float angle = Quaternion.Angle(startRot, targetRot);
        // ����� �� ������� �� �������� ��������
        float duration = angle / rotationSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        transform.rotation = targetRot;
    }

    public Task<bool> TeleportToCell(HexCell targetCell)
    {
        // ���� ��� ��������������� � ���������� ����������
        teleportTcs?.SetResult(false);

        teleportTcs = new TaskCompletionSource<bool>();
        teleportTarget = targetCell;

        // ��������� �������� ������
        animatorController.PlayStartTeleport();

        return teleportTcs.Task;
    }

    public void OnTeleportMove()
    {
        if (teleportTarget == null) return;

        // ��������� ������ �������� �� ����
        currentCell = teleportTarget;
        transform.position = currentCell.transform.position;
    }

    // ���������� ����� Animation Event � EndTeleport (� ������ ������� ���������)
    public void OnTeleportEnd()
    {
        // ��������� �������� �����, ���� �����
        animatorController.PlayEndTeleport();

        // ��������� Task, ����� BattlefieldManager ��������� ������
        teleportTcs?.SetResult(true);
        teleportTcs = null;
        teleportTarget = null;
    }
}
