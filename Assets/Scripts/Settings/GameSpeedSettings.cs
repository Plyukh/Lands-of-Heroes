using System;
using UnityEngine;

/// <summary>
/// Глобальные настройки скорости игры (передвижение + анимации)
/// Синглтон для доступа из любого места
/// </summary>
public class GameSpeedSettings : MonoBehaviour
{
    private static GameSpeedSettings instance;
    public static GameSpeedSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameSpeedSettings>();
            }
            return instance;
        }
    }

    [Header("Speed Multiplier Settings")]
    [SerializeField]
    [Range(1, 3)]
    [Tooltip("Множитель скорости: 1 = нормально, 2 = вдвое быстрее, 3 = втрое быстрее")]
    private int speedMultiplier = 1;

    /// <summary>
    /// Событие, вызываемое при изменении множителя скорости
    /// </summary>
    public event Action<int> OnSpeedMultiplierChanged;

    /// <summary>
    /// Текущий множитель скорости (1 - 3)
    /// </summary>
    public int SpeedMultiplier
    {
        get => speedMultiplier;
        set
        {
            int clampedValue = Mathf.Clamp(value, 1, 3);
            if (speedMultiplier != clampedValue)
            {
                speedMultiplier = clampedValue;
                OnSpeedMultiplierChanged?.Invoke(speedMultiplier);
                
                // Применяем ко всем существующим существам
                ApplyToAllCreatures();
            }
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    
    private void OnDestroy()
    {
        // Очищаем синглтон при уничтожении
        if (instance == this)
        {
            instance = null;
        }
    }

    private void Start()
    {
        // Применяем начальное значение
        ApplyToAllCreatures();
    }

    /// <summary>
    /// Применяет текущий множитель ко всем существующим существам
    /// </summary>
    private void ApplyToAllCreatures()
    {
        var allCreatures = FindObjectsOfType<Creature>();
        foreach (var creature in allCreatures)
        {
            if (creature.Mover != null)
            {
                creature.Mover.UpdateSpeedMultiplier(speedMultiplier);
            }
        }
    }

    /// <summary>
    /// Сбросить скорость к базовому значению (1x)
    /// </summary>
    public void ResetSpeed()
    {
        SpeedMultiplier = 1;
    }
}

