using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProjectileController : MonoBehaviour
{
    public float speed = 10f;

    private Transform target;
    private Action onHit;
    private Vector3 aimPosition;       // ������ ������� ������� ���������
    private bool useDynamicAim;        // ��������� aimPosition ������ ����

    /// <summary>
    /// �������������� � ������ ������ ���������.
    /// </summary>
    public void Initialize(Vector3 worldAimPos, Action onHitCallback)
    {
        target = null;
        useDynamicAim = false;
        aimPosition = worldAimPos;
        onHit = onHitCallback;
    }

    /// <summary>
    /// �������������� � Transform ���� � Y-�������� (0 = ����, 0.5 = �����, 1 = ��� �������).
    /// </summary>
    public void Initialize(Transform target, float heightNormalized, Action onHitCallback)
    {
        this.target = target;
        this.onHit = onHitCallback;
        this.useDynamicAim = true;
        SetAimHeight(heightNormalized);
    }

    private void SetAimHeight(float heightNormalized)
    {
        // ������� ������� ���������� ����
        if (target.TryGetComponent<Collider>(out var col))
        {
            var b = col.bounds;
            // ������ ��������� ����� min.y � max.y
            float y = Mathf.Lerp(b.min.y, b.max.y, heightNormalized);
            aimPosition = new Vector3(b.center.x, y, b.center.z);
        }
        else
        {
            // fallback � ������ target.position + offset
            aimPosition = target.position + Vector3.up * heightNormalized;
        }
    }

    private void Update()
    {
        // ��������� aimPosition, ���� ��� ����� ��������� �� ���� (��������, ���������)
        if (useDynamicAim && target != null)
        {
            SetAimHeight((aimPosition.y - target.position.y)
                         / target.localScale.y);
        }

        // ������� ��������
        Vector3 dir = (aimPosition - transform.position).normalized;
        float step = speed * Time.deltaTime;
        transform.position += dir * step;

        // ��������� ���������
        if (Vector3.Distance(transform.position, aimPosition) <= step)
            HitTarget();
    }

    private void HitTarget()
    {
        onHit?.Invoke();
        Destroy(gameObject);
    }
}