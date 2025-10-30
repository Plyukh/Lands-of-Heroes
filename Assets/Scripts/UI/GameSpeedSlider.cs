using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI компонент для управления скоростью игры через слайдер
/// </summary>
[RequireComponent(typeof(Slider))]
public class GameSpeedSlider : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TextMeshProUGUI speedText;
    
    [Header("Display Settings")]
    [SerializeField] private string textFormat = "Speed: {0}x";

    private void Awake()
    {
        if (speedSlider == null)
            speedSlider = GetComponent<Slider>();
    }

    private void Start()
    {
        // Настройка слайдера
        speedSlider.minValue = 1;
        speedSlider.maxValue = 3;
        speedSlider.wholeNumbers = true; // Только целые числа
        
        // Устанавливаем текущее значение из настроек
        if (GameSpeedSettings.Instance != null)
        {
            speedSlider.value = GameSpeedSettings.Instance.SpeedMultiplier;
        }
        
        // Подписываемся на изменения
        speedSlider.onValueChanged.AddListener(OnSliderValueChanged);
        
        // Обновляем текст
        UpdateSpeedText(Mathf.RoundToInt(speedSlider.value));
    }

    private void OnDestroy()
    {
        if (speedSlider != null)
        {
            speedSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }

    /// <summary>
    /// Вызывается при изменении значения слайдера
    /// </summary>
    private void OnSliderValueChanged(float value)
    {
        int intValue = Mathf.RoundToInt(value);
        
        if (GameSpeedSettings.Instance != null)
        {
            GameSpeedSettings.Instance.SpeedMultiplier = intValue;
        }
        
        UpdateSpeedText(intValue);
    }

    /// <summary>
    /// Обновляет текст с текущей скоростью
    /// </summary>
    private void UpdateSpeedText(int value)
    {
        if (speedText == null)
            return;
        
        speedText.text = string.Format(textFormat, value.ToString());
    }

    /// <summary>
    /// Сбросить скорость к базовому значению (1x)
    /// </summary>
    public void ResetToDefault()
    {
        speedSlider.value = 1;
    }
}

