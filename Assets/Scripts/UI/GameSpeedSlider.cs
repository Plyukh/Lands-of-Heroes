using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI компонент для управления скоростью игры через слайдер.
/// Отображает текущую скорость и позволяет изменять её от 1x до 3x.
/// </summary>
[RequireComponent(typeof(Slider))]
public class GameSpeedSlider : MonoBehaviour
{
    // ========== Константы ==========
    private const int MIN_SPEED = 1;
    private const int MAX_SPEED = 3;
    private const string DEFAULT_TEXT_FORMAT = "Speed: {0}x";

    // ========== Компоненты ==========
    [Header("Компоненты UI")]
    [SerializeField] 
    [Tooltip("Слайдер для изменения скорости")]
    private Slider speedSlider;
    
    [SerializeField] 
    [Tooltip("Текст для отображения текущей скорости (опционально)")]
    private TextMeshProUGUI speedText;

    // ========== Настройки ==========
    [Header("Настройки отображения")]
    [SerializeField] 
    [Tooltip("Формат текста. {0} будет заменено на число скорости")]
    private string textFormat = DEFAULT_TEXT_FORMAT;

    // ========== Unity Callbacks ==========
    private void Awake()
    {
        InitializeSlider();
    }

    private void Start()
    {
        SetupSlider();
        LoadCurrentSpeed();
        SubscribeToEvents();
        UpdateSpeedText();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    // ========== Инициализация ==========
    private void InitializeSlider()
    {
        // Если слайдер не назначен вручную, берем с этого же объекта
        if (speedSlider == null)
            speedSlider = GetComponent<Slider>();
    }

    private void SetupSlider()
    {
        speedSlider.minValue = MIN_SPEED;
        speedSlider.maxValue = MAX_SPEED;
        speedSlider.wholeNumbers = true; // Только целые числа: 1, 2, 3
    }

    private void LoadCurrentSpeed()
    {
        // Загружаем текущее значение из настроек
        if (GameSpeedSettings.Instance != null)
            speedSlider.value = GameSpeedSettings.Instance.SpeedMultiplier;
    }

    private void SubscribeToEvents()
    {
        speedSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void UnsubscribeFromEvents()
    {
        if (speedSlider != null)
            speedSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    // ========== Обработчики событий ==========
    /// <summary>
    /// Вызывается когда игрок двигает слайдер.
    /// </summary>
    private void OnSliderChanged(float value)
    {
        int speedValue = Mathf.RoundToInt(value);
        
        ApplySpeedToSettings(speedValue);
        UpdateSpeedText();
    }

    private void ApplySpeedToSettings(int speed)
    {
        if (GameSpeedSettings.Instance != null)
            GameSpeedSettings.Instance.SpeedMultiplier = speed;
    }

    // ========== Обновление UI ==========
    private void UpdateSpeedText()
    {
        if (speedText == null)
            return;
        
        int currentSpeed = Mathf.RoundToInt(speedSlider.value);
        speedText.text = string.Format(textFormat, currentSpeed);
    }

    // ========== Публичные методы ==========
    /// <summary>
    /// Сбрасывает слайдер к базовой скорости (1x).
    /// Можно вызвать из UI кнопки.
    /// </summary>
    public void ResetToDefault()
    {
        speedSlider.value = MIN_SPEED;
    }
}
