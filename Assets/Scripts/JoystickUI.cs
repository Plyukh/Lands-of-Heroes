using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class JoystickUI : MonoBehaviour
{
    [Header("Main")]
    [Tooltip("Количество действий (число сегментов)")]
    [Range(1, 3)]
    [SerializeField] private int actionCount = 1;
    [Tooltip("Максимальный радиус перемещения ручки в пикселях")]
    [SerializeField] private float maxRadius = 100f;
    [Tooltip("Смещение 0°-угла (в градусах). 0° = вправо, 90° = вверх и т.д.")]
    [SerializeField] private float startAngle = 0f;

    [Header("Knob & Frame")]
    [Tooltip("Список изображений сегментов рамки, в порядке по часовой стрелке от правой стороны")]
    [SerializeField] private List<Image> frameImages;
    [SerializeField] private RectTransform frameRect;
    [SerializeField] private RectTransform knobRect;
    public int ActionCount => actionCount;
    public bool IsReadyToConfirm { get; private set; }
    public Vector2 KnobPositionNormalized { get; private set; }
    public Vector2 KnobScreenPosition => knobRect.position;

    private JoystickActionType[] actionTypes;
    private JoystickActionType currentAction;
    public JoystickActionType CurrentAction => currentAction;

    public void SetActionType(JoystickActionType action, int index)
    {
        if(actionTypes == null)
        {
            actionTypes = new JoystickActionType[actionCount];
        }
        actionTypes[index] = action;
    }

    public void Show(Vector2 screenPos)
    {
        // позиция UI и сброс состояния
        transform.position = screenPos;
        knobRect.anchoredPosition = Vector2.zero;
        KnobPositionNormalized = Vector2.zero;
        IsReadyToConfirm = false;

        // все сегменты красные
        foreach (var img in frameImages)
            img.color = Color.red;

        gameObject.SetActive(true);
    }

    public void UpdateDrag(Vector2 screenPos)
    {
        // 1) локальная точка в рамке
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            frameRect, screenPos, null, out var localPoint);

        // 2) двигаем knob и нормируем
        var clamped = Vector2.ClampMagnitude(localPoint, maxRadius);
        knobRect.anchoredPosition = clamped;
        KnobPositionNormalized = clamped / maxRadius;

        // 3) готовы ли мы подтвердить (ручка на краю)
        bool ready = clamped.magnitude >= maxRadius * 0.99f;
        if (ready != IsReadyToConfirm)
            IsReadyToConfirm = ready;

        // 4) угол в глобальной системе [0..360)
        float angle = Mathf.Atan2(clamped.y, clamped.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        // 5) ищем ближайший сегмент
        int segments = Mathf.Clamp(actionCount, 1, frameImages.Count);
        float slice = 360f / segments;
        float bestDelta = float.MaxValue;
        int bestIdx = 0;

        for (int i = 0; i < segments; i++)
        {
            // центр i-го сегмента
            float center = (startAngle + slice * i + slice * 0.5f) % 360f;
            // минимальное расстояние по кругу
            float delta = Mathf.Abs(Mathf.DeltaAngle(angle, center));
            if (delta < bestDelta)
            {
                bestDelta = delta;
                bestIdx = i;
            }
        }

        // 6) красим: если ready — активный сегмент в зеленый, остальные в красный
        for (int i = 0; i < frameImages.Count; i++)
        {
            bool isActive = IsReadyToConfirm && i == bestIdx;
            frameImages[i].color = isActive ? Color.green : Color.red;

            if (actionCount > 1)
            {
                if (frameImages[i].color == Color.green)
                {
                    currentAction = actionTypes[i];
                    print(currentAction);
                }
            }
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}