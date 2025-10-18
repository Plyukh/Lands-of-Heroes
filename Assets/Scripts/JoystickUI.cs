using System.Collections.Generic;
using System.Linq;
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

    public int ActionCount => actionCount;
    public bool IsReadyToConfirm { get; private set; }
    public Vector2 KnobPositionNormalized { get; private set; }
    public Vector2 KnobScreenPosition => knobRect.position;
    public JoystickActionType CurrentAction => currentAction;

    public void SetActionType(JoystickActionType action, int index)
    {
        if (actionTypes == null || actionTypes.Length != actionCount)
        {
            actionTypes = new JoystickActionType[actionCount];

            // Создаем словарь для быстрого доступа к иконкам по типу действия
            iconMap = icons.ToDictionary(data => data.actionType);
            if (actionTypes == null || actionTypes.Length != actionCount)
            {
                actionTypes = new JoystickActionType[actionCount];
            }
        }
        if (index >= 0 && index < actionCount)
        {
            actionTypes[index] = action;
        }
    }

    public void Show(Vector2 screenPos)
    {
        // позиция UI и сброс состояния
        transform.position = screenPos;
        knobRect.anchoredPosition = Vector2.zero;
        KnobPositionNormalized = Vector2.zero;
        IsReadyToConfirm = false;

        // Настраиваем видимость и внешний вид сегментов
        for (int i = 0; i < frameImages.Count; i++)
        {
            bool isSegmentActive = i < actionCount;

            frameImages[i].gameObject.SetActive(isSegmentActive);
            backgroundImages[i].gameObject.SetActive(isSegmentActive);
            iconImages[i].gameObject.SetActive(isSegmentActive);

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

        // 6) красим сегменты
        for (int i = 0; i < segments; i++)
        {
            if (i >= actionTypes.Length) continue;

            JoystickActionType type = actionTypes[i];
            if (iconMap.TryGetValue(type, out var iconData))
            {
                bool isSegmentSelected = IsReadyToConfirm && i == bestIdx;
                Color targetColor = isSegmentSelected ? iconData.enabledColor : iconData.disabledColor;

                frameImages[i].color = targetColor;
                backgroundImages[i].color = targetColor;

                if (isSegmentSelected)
                {
                    currentAction = actionTypes[i];
                }
            }
        }
    }

    public void Hide()
    {
        joystickAnimatorController.PlayJoystickClose();
    }
}