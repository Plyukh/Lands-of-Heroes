using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProjectileController : MonoBehaviour
{
    // �������� �����
    public float speed = 10f;

    // ���� �����
    private Transform target;
    private Action onHit;

    /// <summary>
    /// ��������� �������� � ���� � �������� onHit ��� ����������.
    /// </summary>
    public void Initialize(Transform target, Action onHitCallback)
    {
        this.target = target;
        onHit = onHitCallback;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // ������� ������ � ����� ����
        Vector3 dir = (target.position - transform.position).normalized;
        float step = speed * Time.deltaTime;
        transform.position += dir * step;

        // ���� ��� ������ � �������, ��� ������
        if (Vector3.Distance(transform.position, target.position) <= step)
            HitTarget();
    }

    private void HitTarget()
    {
        onHit?.Invoke();
        Destroy(gameObject);
    }
}