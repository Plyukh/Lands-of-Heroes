using UnityEngine;
using UnityEngine.UI;

public class JoystickUI : MonoBehaviour
{
    [Header("UI Images")]
    [SerializeField] private Image bg;
    [SerializeField] private Image knob;

    [Header("Settings")]
    [Tooltip("Pixels from center to edge of the joystick frame")]
    [SerializeField] private float maxRadius = 100f;

    public bool IsReadyToConfirm { get; private set; }
    public Vector2 KnobPositionNormalized { get; private set; }
    // реально берём экранные координаты knob’а
    public Vector2 KnobScreenPosition => knobRect.position;

    private RectTransform bgRect;
    private RectTransform knobRect;

    private void Awake()
    {
        bgRect = bg.GetComponent<RectTransform>();
        knobRect = knob.GetComponent<RectTransform>();
        Hide();
    }

    public void Show(Vector2 screenPos)
    {
        transform.position = screenPos;
        knobRect.anchoredPosition = Vector2.zero;
        KnobPositionNormalized = Vector2.zero;
        IsReadyToConfirm = false;
        bg.color = Color.red;
        gameObject.SetActive(true);
    }

    public void UpdateDrag(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            bgRect, screenPos, null, out var localPoint);

        var clamped = Vector2.ClampMagnitude(localPoint, maxRadius);
        knobRect.anchoredPosition = clamped;
        KnobPositionNormalized = clamped / maxRadius;

        bool ready = clamped.magnitude >= maxRadius * 0.99f;
        if (ready != IsReadyToConfirm)
        {
            IsReadyToConfirm = ready;
            bg.color = ready ? Color.green : Color.red;
        }
    }

    public void Hide() => gameObject.SetActive(false);
}
