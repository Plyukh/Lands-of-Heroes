# Система настройки скорости игры - Быстрый старт

## 🎮 Что это?

Позволяет игроку менять скорость игры через UI слайдер:
- **1x** - нормальная скорость
- **2x** - вдвое быстрее  
- **3x** - втрое быстрее

Влияет на:
- ✅ Скорость передвижения существ
- ✅ Скорость поворота существ
- ✅ Скорость всех анимаций

---

## 📦 Быстрая установка (5 минут)

### Шаг 1: Создать объект настроек
```
1. В UI создайте пустой GameObject
2. Назовите его "Settings"
3. Добавьте компонент GameSpeedSettings
```

### Шаг 2: Создать слайдер
```
1. В UI создайте Slider (ПКМ на Canvas → UI → Slider)
2. Назовите его "SpeedSlider"
3. Добавьте компонент GameSpeedSlider
4. В инспекторе GameSpeedSlider:
   - Speed Slider: перетащите сам Slider
   - Speed Text: (опционально) создайте TextMeshPro текст
```

### Шаг 3: Готово! ✨
Запустите игру и подвигайте слайдер - скорость изменится!

---

## 💡 Использование в коде

### Получить текущую скорость
```csharp
int currentSpeed = GameSpeedSettings.Instance.SpeedMultiplier;
Debug.Log($"Текущая скорость: {currentSpeed}x");
```

### Изменить скорость программно
```csharp
// Установить скорость 2x
GameSpeedSettings.Instance.SpeedMultiplier = 2;

// Сбросить к нормальной скорости
GameSpeedSettings.Instance.ResetSpeed();
```

### Подписаться на изменения
```csharp
void Start()
{
    GameSpeedSettings.Instance.OnSpeedMultiplierChanged += OnSpeedChanged;
}

void OnSpeedChanged(int newSpeed)
{
    Debug.Log($"Скорость изменилась на {newSpeed}x");
}

void OnDestroy()
{
    GameSpeedSettings.Instance.OnSpeedMultiplierChanged -= OnSpeedChanged;
}
```

---

## 🏗️ Архитектура

### Как это работает?

```
GameSpeedSettings (синглтон)
    ↓ событие OnSpeedMultiplierChanged
CreatureMover (у каждого существа)
    ↓ вызывает SetAnimationSpeed()
CreatureAnimatorController
    ↓ устанавливает animator.speed
Анимации играют быстрее!
```

### Поток изменения скорости:

1. **Игрок двигает слайдер** → `GameSpeedSlider.OnSliderChanged()`
2. **Обновляется настройка** → `GameSpeedSettings.SpeedMultiplier = newValue`
3. **Вызывается событие** → `OnSpeedMultiplierChanged?.Invoke(newValue)`
4. **Все существа обновляются** → `CreatureMover.UpdateSpeedMultiplier(newValue)`
5. **Применяется к анимациям** → `animator.speed = newValue`

---

## 📁 Файлы системы

| Файл | Назначение |
|------|-----------|
| `GameSpeedSettings.cs` | Глобальный менеджер настроек (синглтон) |
| `GameSpeedSlider.cs` | UI компонент для слайдера |
| `CreatureMover.cs` | Применяет множитель к движению |
| `CreatureAnimatorController.cs` | Применяет множитель к анимациям |

---

## ⚙️ Настройки компонентов

### GameSpeedSettings
- **Speed Multiplier** - начальная скорость (1-3)

### GameSpeedSlider  
- **Speed Slider** - ссылка на компонент Slider
- **Speed Text** - (опционально) текст для отображения
- **Text Format** - формат текста (по умолчанию "Speed: {0}x")

---

## 🐛 Частые проблемы

### Скорость не применяется
✅ Убедитесь что `GameSpeedSettings` есть в сцене  
✅ Проверьте что существа имеют `CreatureMover`

### Ошибка при закрытии сцены
✅ Убедитесь что в коде НЕТ `DontDestroyOnLoad`  
✅ `GameSpeedSettings` должен быть частью сцены

### Слайдер не работает
✅ Проверьте что `Speed Slider` назначен в инспекторе  
✅ Убедитесь что `GameSpeedSettings` существует в сцене

---

## 🎨 Кастомизация

### Изменить диапазон скорости
Отредактируйте константы в `GameSpeedSettings.cs`:
```csharp
private const int MIN_SPEED = 1;
private const int MAX_SPEED = 3; // Измените на 5 для диапазона 1-5
```

### Изменить формат текста
В инспекторе `GameSpeedSlider`:
```
Text Format: "Скорость: {0}x"
Text Format: "x{0}"
Text Format: "{0} раз быстрее"
```

---

## 📝 Примечания

- ✅ Множитель применяется автоматически ко всем существам
- ✅ Новые существа подхватывают текущую скорость при создании
- ✅ Система работает в редакторе и в билде
- ⚠️ Настройка НЕ сохраняется между сессиями (сбрасывается на 1x)

---

Подробная документация: `GameSpeedSettings_README.md`

