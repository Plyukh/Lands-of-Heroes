using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProjectileController : MonoBehaviour
{
    // Скорость полёта
    public float speed = 10f;

    // Цель полёта
    private Transform target;
    private Action onHit;

    /// <summary>
    /// Запускает движение к цели и вызывает onHit при достижении.
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

        // Двигаем снаряд к точке цели
        Vector3 dir = (target.position - transform.position).normalized;
        float step = speed * Time.deltaTime;
        transform.position += dir * step;

        // Если уже близко — считаем, что попали
        if (Vector3.Distance(transform.position, target.position) <= step)
            HitTarget();
    }

    private void HitTarget()
    {
        onHit?.Invoke();
        Destroy(gameObject);
    }
}