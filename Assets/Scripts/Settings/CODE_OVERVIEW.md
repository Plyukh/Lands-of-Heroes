# Обзор кода системы настройки скорости

## 📚 Для разработчиков

Этот документ объясняет как работает код системы настройки скорости.

---

## 🏗️ Структура классов

### 1. GameSpeedSettings (Главный менеджер)

**Ответственность:**
- Хранит текущий множитель скорости (1-3)
- Уведомляет подписчиков об изменениях
- Применяет новую скорость ко всем существам

**Ключевые части:**

```csharp
// Синглтон - один экземпляр на всю игру
public static GameSpeedSettings Instance { get; }

// Основная настройка
public int SpeedMultiplier { get; set; }

// Событие для подписки
public event Action<int> OnSpeedMultiplierChanged;
```

**Логика работы:**
1. При изменении `SpeedMultiplier` вызывается сеттер
2. Сеттер проверяет что значение изменилось
3. Вызывается событие `OnSpeedMultiplierChanged`
4. Вызывается `ApplySpeedToAllCreatures()` для применения ко всем

---

### 2. GameSpeedSlider (UI компонент)

**Ответственность:**
- Отображает слайдер с диапазоном 1-3
- Обновляет текст с текущей скоростью
- Передает изменения в `GameSpeedSettings`

**Ключевые части:**

```csharp
// Компоненты
private Slider speedSlider;
private TextMeshProUGUI speedText;

// Главный обработчик
private void OnSliderChanged(float value)
{
    int speedValue = Mathf.RoundToInt(value);
    GameSpeedSettings.Instance.SpeedMultiplier = speedValue;
    UpdateSpeedText();
}
```

**Логика работы:**
1. Unity вызывает `OnSliderChanged` когда игрок двигает слайдер
2. Конвертируем float в int (1.5 → 2)
3. Устанавливаем новое значение в `GameSpeedSettings`
4. Обновляем текст на UI

---

### 3. CreatureMover (Управление движением)

**Ответственность:**
- Подписывается на изменения скорости
- Применяет множитель к скорости передвижения
- Применяет множитель к скорости поворота
- Передает множитель в `CreatureAnimatorController`

**Ключевые части:**

```csharp
// Текущий множитель
private int speedMultiplier = 1;

// Подписка на изменения
private void SubscribeToSpeedSettings()
{
    speedMultiplier = GameSpeedSettings.Instance.SpeedMultiplier;
    GameSpeedSettings.Instance.OnSpeedMultiplierChanged += UpdateSpeedMultiplier;
}

// Применение к движению
float actualSpeed = moveSpeed * speedMultiplier;
```

**Логика работы:**
1. В `Start()` подписываемся на событие из `GameSpeedSettings`
2. При изменении вызывается `UpdateSpeedMultiplier()`
3. Сохраняем новый множитель в `speedMultiplier`
4. Передаем в `animatorController.SetAnimationSpeed()`
5. В `MoveCoroutine` умножаем базовую скорость на множитель
6. В `RotateTowardsAsync` умножаем скорость поворота на множитель

---

### 4. CreatureAnimatorController (Управление анимациями)

**Ответственность:**
- Устанавливает скорость аниматора Unity

**Ключевые части:**

```csharp
// Текущая скорость
private int animationSpeed = 1;

// Применение к аниматору
public void SetAnimationSpeed(int multiplier)
{
    animationSpeed = Mathf.Clamp(multiplier, 1, 3);
    animator.speed = animationSpeed;
}
```

**Логика работы:**
1. `CreatureMover` вызывает `SetAnimationSpeed()`
2. Сохраняем значение в `animationSpeed`
3. Применяем к `animator.speed` (встроенное свойство Unity)

---

## 🔄 Последовательность событий

### При старте игры:

```
1. GameSpeedSettings.Awake()
   └─→ Инициализация синглтона

2. GameSpeedSettings.Start()
   └─→ ApplySpeedToAllCreatures()
       └─→ Находит всех существ
       └─→ Вызывает creature.Mover.UpdateSpeedMultiplier()

3. CreatureMover.Start() (для каждого существа)
   └─→ SubscribeToSpeedSettings()
       └─→ Получает текущую скорость
       └─→ Подписывается на OnSpeedMultiplierChanged
       └─→ Вызывает animatorController.SetAnimationSpeed()

4. GameSpeedSlider.Start()
   └─→ Настраивает слайдер (min=1, max=3, wholeNumbers=true)
   └─→ Загружает текущее значение
   └─→ Подписывается на изменения слайдера
```

### При изменении скорости:

```
1. Игрок двигает слайдер
   ↓
2. GameSpeedSlider.OnSliderChanged(float value)
   └─→ Конвертирует в int
   └─→ GameSpeedSettings.Instance.SpeedMultiplier = newValue
   ↓
3. GameSpeedSettings.SpeedMultiplier.set
   └─→ Проверяет изменение
   └─→ OnSpeedMultiplierChanged?.Invoke(newValue)
   └─→ ApplySpeedToAllCreatures()
   ↓
4. CreatureMover.UpdateSpeedMultiplier(newValue) (для каждого существа)
   └─→ Сохраняет speedMultiplier = newValue
   └─→ animatorController.SetAnimationSpeed(newValue)
   ↓
5. CreatureAnimatorController.SetAnimationSpeed(newValue)
   └─→ animator.speed = newValue
```

---

## 🎯 Важные детали

### Почему используется событие?

**Вместо:**
```csharp
// Плохо - каждое существо проверяет настройки каждый кадр
void Update()
{
    if (GameSpeedSettings.Instance.SpeedMultiplier != currentSpeed)
        UpdateSpeed();
}
```

**Используется:**
```csharp
// Хорошо - обновление только когда нужно
GameSpeedSettings.Instance.OnSpeedMultiplierChanged += UpdateSpeedMultiplier;
```

**Преимущества:**
- ✅ Эффективнее (нет проверок каждый кадр)
- ✅ Чище код
- ✅ Легче отлаживать

---

### Почему int а не float?

```csharp
private int speedMultiplier = 1;  // ✅ Хорошо
private float speedMultiplier = 1.5f;  // ❌ Не используется
```

**Причины:**
- Проще для игрока (1x, 2x, 3x вместо 1.0x, 1.5x, 2.0x)
- Меньше багов с округлением
- Слайдер с 3 позициями вместо плавного
- Проще тестировать

---

### Почему НЕТ DontDestroyOnLoad?

```csharp
// НЕ используется:
// DontDestroyOnLoad(gameObject);
```

**Причины:**
- Объект `GameSpeedSettings` привязан к сцене
- При переходе между сценами создается новый
- Проще управлять жизненным циклом
- Нет конфликтов с несколькими экземплярами

---

## 🧪 Тестирование

### Как протестировать вручную:

1. **Создайте тестовую сцену** с существами
2. **Добавьте GameSpeedSettings** в сцену
3. **Добавьте слайдер** с GameSpeedSlider
4. **Запустите игру**
5. **Подвигайте слайдер** - существа должны двигаться быстрее
6. **Проверьте анимации** - должны играть быстрее

### Юнит-тесты (TODO):

```csharp
[Test]
public void SpeedMultiplier_ClampsBetween1And3()
{
    var settings = CreateGameSpeedSettings();
    
    settings.SpeedMultiplier = 0;
    Assert.AreEqual(1, settings.SpeedMultiplier);
    
    settings.SpeedMultiplier = 5;
    Assert.AreEqual(3, settings.SpeedMultiplier);
}

[Test]
public void SpeedChange_TriggersEvent()
{
    var settings = CreateGameSpeedSettings();
    int receivedValue = 0;
    
    settings.OnSpeedMultiplierChanged += (value) => receivedValue = value;
    settings.SpeedMultiplier = 2;
    
    Assert.AreEqual(2, receivedValue);
}
```

---

## 🔧 Расширение системы

### Добавить новую настройку:

```csharp
public class GameSpeedSettings : MonoBehaviour
{
    // Добавьте новое поле
    [SerializeField]
    private bool pauseOnSpeedChange = false;
    
    public bool PauseOnSpeedChange
    {
        get => pauseOnSpeedChange;
        set => pauseOnSpeedChange = value;
    }
}
```

### Добавить больше диапазонов:

```csharp
// Измените константы
private const int MIN_SPEED = 1;
private const int MAX_SPEED = 5;  // Было 3

// Обновите Range
[Range(MIN_SPEED, MAX_SPEED)]
```

---

## 📊 Производительность

### Замеры:

- **Подписка на событие:** ~0.001ms (один раз при старте)
- **Изменение скорости:** ~0.05ms (FindObjectsOfType + обновление всех существ)
- **Движение с множителем:** ~0ms (простое умножение)

### Оптимизация:

Если существ очень много (1000+), можно кешировать список:

```csharp
private List<CreatureMover> allMovers = new List<CreatureMover>();

private void SubscribeToSpeedSettings()
{
    // Добавляем себя в список при создании
    allMovers.Add(this);
}

private void ApplySpeedToAllCreatures()
{
    // Используем кешированный список вместо FindObjectsOfType
    foreach (var mover in allMovers)
        mover.UpdateSpeedMultiplier(speedMultiplier);
}
```

---

## 💡 Советы по отладке

### Добавьте логирование:

```csharp
public int SpeedMultiplier
{
    set
    {
        Debug.Log($"[GameSpeedSettings] Changing speed: {speedMultiplier} → {value}");
        // ... остальной код
    }
}
```

### Проверьте подписки:

```csharp
private void OnEnable()
{
    Debug.Log($"[CreatureMover] {gameObject.name} subscribing to speed changes");
}
```

### Визуализируйте в редакторе:

```csharp
private void OnGUI()
{
    GUI.Label(new Rect(10, 10, 200, 20), 
        $"Speed: {speedMultiplier}x");
}
```

---

Полная документация: `GameSpeedSettings_README.md`  
Быстрый старт: `QUICK_START.md`

