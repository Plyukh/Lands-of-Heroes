using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProjectileController : MonoBehaviour
{
    [Header("Flight Settings")]
    [Tooltip("�������� ����� (������ Unity � �������)")]
    [SerializeField] private float speed = 10f;

    [Header("Optional Impact Effect")]
    [Tooltip("������ ������� ��� ��������� (�������� ������, ���� �� �����)")]
    [SerializeField] private GameObject impactEffectPrefab;

    private Transform target;
    private Action onHitCallback;         // ������ � ����, �������� PlayImpact
    private Action onCompleteCallback;    // ����������� ���, �������� ��������� ����������
    private Vector3 aimPosition;
    private bool useDynamicAim;
    private float heightNormalized;

    /// <summary>
    /// �������������� � ������ ������� ������ ���������.
    /// </summary>
    public void Initialize(Vector3 worldAimPos, Action onHit, Action onComplete = null)
    {
        this.target = null;
        this.useDynamicAim = false;
        this.aimPosition = worldAimPos;
        this.onHitCallback = onHit;
        this.onCompleteCallback = onComplete;
    }

    /// <summary>
    /// �������������� � Transform ���� � Y-���������:
    /// heightNormalized � ��������� [0,1] (0 � ����, 0.5 � �����, 1 � ������).
    /// </summary>
    public void Initialize(Transform target, float heightNormalized, Action onHit, Action onComplete = null)
    {
        this.target = target;
        this.onHitCallback = onHit;
        this.onCompleteCallback = onComplete;
        this.useDynamicAim = true;
        this.heightNormalized = Mathf.Clamp01(heightNormalized);

        UpdateAimPosition();
    }

    private void Update()
    {
        // ���� ���� ���������� � ������� ������
        if (useDynamicAim && (target == null))
        {
            Destroy(gameObject);
            return;
        }

        // ��� ������������� ������������ ������ ���� ��������� aimPosition
        if (useDynamicAim)
            UpdateAimPosition();

        // ������� ������ � aimPosition
        Vector3 dir = (aimPosition - transform.position).normalized;
        float step = speed * Time.deltaTime;
        transform.position += dir * step;

        // ��������� ��������� �� ���������
        if (Vector3.Distance(transform.position, aimPosition) <= step)
            HitTarget();
    }

    /// <summary>
    /// ��������� aimPosition �� ���������� ���� � heightNormalized.
    /// </summary>
    private void UpdateAimPosition()
    {
        if (target != null && target.TryGetComponent<Collider>(out var col))
        {
            var bounds = col.bounds;
            float y = Mathf.Lerp(bounds.min.y, bounds.max.y, heightNormalized);
            aimPosition = new Vector3(bounds.center.x, y, bounds.center.z);
        }
        else if (target != null)
        {
            // fallback: ����� + �������� �� ������
            aimPosition = target.position + Vector3.up * heightNormalized;
        }
    }

    private void HitTarget()
    {
        // 1) ������� ������������ ������ ��������� (�� callback'�� ��� ����� � �� �������)
        if (impactEffectPrefab != null)
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);

        // 2) ������ callback: ������ � ���� (��������, PlayImpact)
        try { onHitCallback?.Invoke(); }
        catch (Exception ex) { Debug.LogException(ex); }

        // 3) ������ callback: ����������� ��� (��������, ��������� �������� ���������� -> �� ������� OnAttackHit)
        try { onCompleteCallback?.Invoke(); }
        catch (Exception ex) { Debug.LogException(ex); }

        // 4) ������� ������
        Destroy(gameObject);
    }
}
