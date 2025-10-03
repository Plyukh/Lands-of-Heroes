using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProjectileController : MonoBehaviour
{
    public float speed = 10f;

    private Transform target;
    private Action onHit;
    private Vector3 aimPosition;       // точная мировая позиция попадания
    private bool useDynamicAim;        // обновляем aimPosition каждый кадр

    /// <summary>
    /// Инициализирует с жёсткой точкой попадания.
    /// </summary>
    public void Initialize(Vector3 worldAimPos, Action onHitCallback)
    {
        target = null;
        useDynamicAim = false;
        aimPosition = worldAimPos;
        onHit = onHitCallback;
    }

    /// <summary>
    /// Инициализирует с Transform цели и Y-смещение (0 = ноги, 0.5 = центр, 1 = над головой).
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
        // находим границы коллайдера цели
        if (target.TryGetComponent<Collider>(out var col))
        {
            var b = col.bounds;
            // высота попадания между min.y и max.y
            float y = Mathf.Lerp(b.min.y, b.max.y, heightNormalized);
            aimPosition = new Vector3(b.center.x, y, b.center.z);
        }
        else
        {
            // fallback — просто target.position + offset
            aimPosition = target.position + Vector3.up * heightNormalized;
        }
    }

    private void Update()
    {
        // обновляем aimPosition, если нам нужно «следить» за цель (например, двигается)
        if (useDynamicAim && target != null)
        {
            SetAimHeight((aimPosition.y - target.position.y)
                         / target.localScale.y);
        }

        // считаем движение
        Vector3 dir = (aimPosition - transform.position).normalized;
        float step = speed * Time.deltaTime;
        transform.position += dir * step;

        // проверяем попадание
        if (Vector3.Distance(transform.position, aimPosition) <= step)
            HitTarget();
    }

    private void HitTarget()
    {
        onHit?.Invoke();
        Destroy(gameObject);
    }
}