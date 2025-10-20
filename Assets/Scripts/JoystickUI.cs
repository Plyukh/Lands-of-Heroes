using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class JoystickUI : MonoBehaviour
{
    [Header("Main")]
    [Tooltip("Максимальный радиус перемещения ручки в пикселях")]
    [SerializeField] private float maxRadius = 100f;
    [Tooltip("Смещение 0°-угла (в градусах). 0° = вправо, 90° = вверх и т.д.")]
    [SerializeField] private float startAngle = 0f;

    [Header("Animator")]
    [SerializeField] private JoystickAnimatorController joystickAnimatorController;

    [Header("Knob & Frame")]
    [Tooltip("Список изображений сегментов рамки, в порядке по часовой стрелке")]
    [SerializeField] private List<Image> frameImages;
    [Tooltip("Список изображений фона для каждого сегмента")]
    [SerializeField] private List<Image> backgroundImages;
    [Tooltip("Список изображений иконок для каждого сегмента")]
    [SerializeField] private List<Image> iconImages;
    [SerializeField] private RectTransform frameRect;
    [SerializeField] private RectTransform knobRect;

    [Header("Icons Config")]
    [SerializeField] private List<ActionIconData> icons;

    private JoystickActionType[] actionTypes;
    private JoystickActionType currentAction;
    private Dictionary<JoystickActionType, ActionIconData> iconMap;

    public bool IsReadyToConfirm { get; private set; }
    public Vector2 KnobPositionNormalized { get; private set; }
    public Vector2 KnobScreenPosition => knobRect.position;
    public JoystickActionType CurrentAction => currentAction;
    public int GetActionCount()
    {
        if (actionTypes[0] != actionTypes[1])
            return 2;
        return 1;
    }

    public void Initialize()
    {
        actionTypes = new JoystickActionType[2];

        iconMap = icons.ToDictionary(data => data.actionType);
        if (actionTypes == null || actionTypes.Length != actionTypes.Count())
        {
            actionTypes = new JoystickActionType[actionTypes.Count()];
        }
    }

    public void SetActionType(JoystickActionType action, int index)
    {
        if (index >= 0 && index < actionTypes.Count())
        {
            actionTypes[index] = action;
        }
    }

    public void SetAllSegmentsToAction(JoystickActionType action)
    {
        for (int i = 0; i < actionTypes.Length; i++)
        {
            actionTypes[i] = action;
        }
        // Обновляем текущее действие, так как все сегменты теперь одинаковы
        currentAction = action;
    }

    public void SetAnimatorForActionCount(int count, bool value)
    {
        if (joystickAnimatorController != null)
        {
            joystickAnimatorController.SetAction(count, value);
        }
    }

    public void Show(Vector2 screenPos, int actionCount)
    {
        // позиция UI и сброс состояния
        transform.position = screenPos;
        knobRect.anchoredPosition = Vector2.zero;
        KnobPositionNormalized = Vector2.zero;
        IsReadyToConfirm = false;

        // Настраиваем видимость и внешний вид сегментов
        for (int i = 0; i < frameImages.Count; i++)
        {
            bool isSegmentActive = false;

            if(actionCount == 1)
            {
                actionTypes[i] = actionTypes[0];
                isSegmentActive = true;
            }
            else
            {
                isSegmentActive = i < actionCount;
            }

            if (isSegmentActive && i < actionTypes.Length)
            {
                JoystickActionType type = actionTypes[i];
                if (iconMap.TryGetValue(type, out var iconData))
                {
                    iconImages[i].sprite = iconData.iconSprite;
                    iconImages[i].rectTransform.anchoredPosition = iconData.rect.position;
                    iconImages[i].rectTransform.sizeDelta = iconData.rect.size;
                    // Устанавливаем цвет "выключено" для всех элементов сегмента
                    frameImages[i].color = iconData.disabledColor;
                    backgroundImages[i].color = iconData.disabledColor;
                }
            }
        }

        gameObject.SetActive(true);
        joystickAnimatorController.SelectJoystick(actionCount);
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
        int segments = Mathf.Clamp(actionTypes.Count(), 1, frameImages.Count);
        float slice = 360f / segments;
        float bestDelta = float.MaxValue;
        int bestIdx = -1;

        for (int i = 0; i < segments; i++)
        {
            float center = (startAngle + slice * i + slice * 0.5f) % 360f;
            float delta = Mathf.Abs(Mathf.DeltaAngle(angle, center));
            if (delta < bestDelta)
            {
                bestDelta = delta;
                bestIdx = i;
            }
        }

        // 6) Перерисовываем все активные сегменты
        // Проверяем, одинаковы ли все actionTypes в диапазоне [0..segments)
        bool allSame = true;
        if (segments > 1 && actionTypes.Length >= segments)
        {
            var first = actionTypes[0];
            for (int i = 1; i < segments; i++)
            {
                if (actionTypes[i] != first)
                {
                    allSame = false;
                    break;
                }
            }
        }
        else if (segments <= 1)
        {
            allSame = false;
        }

        for (int i = 0; i < segments; i++)
        {
            if (i >= actionTypes.Length) continue;

            JoystickActionType type = actionTypes[i];
            if (!iconMap.TryGetValue(type, out var iconData)) continue;

            bool highlightAll = allSame && IsReadyToConfirm;
            bool highlightSingle = !allSame && IsReadyToConfirm && i == bestIdx;

            bool isHighlighted = highlightAll || highlightSingle;
            Color targetColor = isHighlighted ? iconData.enabledColor : iconData.disabledColor;

            frameImages[i].color = targetColor;
            backgroundImages[i].color = targetColor;

            // Назначаем currentAction только один раз
            if (isHighlighted && (highlightSingle || (highlightAll)))
            {
                currentAction = type;
            }
        }
    }

    public void Hide()
    {
        joystickAnimatorController.PlayJoystickClose(GetActionCount());
        SetAnimatorForActionCount(1, false);
    }
}