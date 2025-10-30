using System;
using UnityEngine;

/// <summary>
/// Управляет глобальной скоростью игры.
/// Влияет на скорость передвижения существ и их анимации.
/// Значения: 1x (нормально), 2x (быстрее), 3x (самое быстрое).
/// </summary>
public class GameSpeedSettings : MonoBehaviour
{
    // ========== Константы ==========
    private const int MIN_SPEED = 1;
    private const int MAX_SPEED = 3;
    private const int DEFAULT_SPEED = 1;

    // ========== Синглтон ==========
    private static GameSpeedSettings instance;
    
    public static GameSpeedSettings Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GameSpeedSettings>();
            
            return instance;
        }
    }

    // ========== Настройки ==========
    [Header("Настройки скорости")]
    [SerializeField]
    [Range(MIN_SPEED, MAX_SPEED)]
    [Tooltip("1 = нормально, 2 = вдвое быстрее, 3 = втрое быстрее")]
    private int speedMultiplier = DEFAULT_SPEED;

    // ========== События ==========
    /// <summary>
    /// Вызывается при изменении скорости.
    /// Параметр: новое значение множителя (1-3).
    /// </summary>
    public event Action<int> OnSpeedMultiplierChanged;

    // ========== Свойства ==========
    /// <summary>
    /// Текущий множитель скорости (1, 2 или 3).
    /// При изменении автоматически применяется ко всем существам.
    /// </summary>
    public int SpeedMultiplier
    {
        get => speedMultiplier;
        set
        {
            int newValue = Mathf.Clamp(value, MIN_SPEED, MAX_SPEED);
            
            if (speedMultiplier == newValue)
                return; // Значение не изменилось
            
            speedMultiplier = newValue;
            OnSpeedMultiplierChanged?.Invoke(speedMultiplier);
            ApplySpeedToAllCreatures();
        }
    }

    // ========== Unity Callbacks ==========
    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        ApplySpeedToAllCreatures();
    }

    private void OnDestroy()
    {
        CleanupSingleton();
    }

    // ========== Инициализация ==========
    private void InitializeSingleton()
    {
        // Если уже есть другой экземпляр - уничтожаем этот
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
    }

    private void CleanupSingleton()
    {
        if (instance == this)
            instance = null;
    }

    // ========== Применение скорости ==========
    /// <summary>
    /// Применяет текущую скорость ко всем существам на сцене.
    /// </summary>
    private void ApplySpeedToAllCreatures()
    {
        Creature[] allCreatures = FindObjectsOfType<Creature>();
        
        foreach (Creature creature in allCreatures)
        {
            if (creature.Mover != null)
                creature.Mover.UpdateSpeedMultiplier(speedMultiplier);
        }
    }

    // ========== Публичные методы ==========
    /// <summary>
    /// Сбрасывает скорость к нормальной (1x).
    /// </summary>
    public void ResetSpeed()
    {
        SpeedMultiplier = DEFAULT_SPEED;
    }
}
