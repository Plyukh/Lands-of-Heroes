using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProjectileController : MonoBehaviour
{
    [Header("Flight Settings")]
    [Tooltip("Скорость полёта (единиц Unity в секунду)")]
    [SerializeField] private float speed = 10f;

    [Header("Optional Impact Effect")]
    [Tooltip("Префаб эффекта при попадании (оставьте пустым, если не нужен)")]
    [SerializeField] private GameObject impactEffectPrefab;

    private Transform target;
    private Action onHitCallback;
    private Vector3 aimPosition;
    private bool useDynamicAim;
    private float heightNormalized;

    /// <summary>
    /// Инициализирует с жёсткой мировой точкой попадания.
    /// </summary>
    public void Initialize(Vector3 worldAimPos, Action onHit)
    {
        this.target = null;
        this.useDynamicAim = false;
        this.aimPosition = worldAimPos;
        this.onHitCallback = onHit;
    }

    /// <summary>
    /// Инициализирует с Transform цели и Y-смещением:
    /// heightNormalized в диапазоне [0,1] (0 — ноги, 0.5 — центр, 1 — голова).
    /// </summary>
    public void Initialize(Transform target, float heightNormalized, Action onHit)
    {
        this.target = target;
        this.onHitCallback = onHit;
        this.useDynamicAim = true;
        this.heightNormalized = Mathf.Clamp01(heightNormalized);

        UpdateAimPosition();
    }

    private void Update()
    {
        // Если цель уничтожена — удаляем снаряд
        if (useDynamicAim && (target == null))
        {
            Destroy(gameObject);
            return;
        }

        // Для динамического прицеливания каждый кадр обновляем aimPosition
        if (useDynamicAim)
            UpdateAimPosition();

        // Двигаем снаряд к aimPosition
        Vector3 dir = (aimPosition - transform.position).normalized;
        float step = speed * Time.deltaTime;
        transform.position += dir * step;

        // Проверяем попадание по дистанции
        if (Vector3.Distance(transform.position, aimPosition) <= step)
            HitTarget();
    }

    /// <summary>
    /// Вычисляет aimPosition по коллайдеру цели и heightNormalized.
    /// </summary>
    private void UpdateAimPosition()
    {
        if (target.TryGetComponent<Collider>(out var col))
        {
            var bounds = col.bounds;
            float y = Mathf.Lerp(bounds.min.y, bounds.max.y, heightNormalized);
            aimPosition = new Vector3(bounds.center.x, y, bounds.center.z);
        }
        else
        {
            // fallback: центр + смещение по высоте
            aimPosition = target.position + Vector3.up * heightNormalized;
        }
    }

    private void HitTarget()
    {
        // 1) Спавним опциональный эффект попадания
        if (impactEffectPrefab != null)
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);

        // 2) Вызываем callback (например, PlayImpact)
        onHitCallback?.Invoke();

        // 3) Удаляем снаряд
        Destroy(gameObject);
    }
}
