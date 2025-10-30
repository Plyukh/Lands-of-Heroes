# UI Очереди ходов - Руководство по настройке

## 🎯 Что это?

Система отображает очередь ходов существ в виде 10 иконок внизу экрана:
- **Первая иконка** (самая левая) - больше размером, желтая рамка (текущий ход)
- **Остальные иконки** - нормальный размер, зеленая (союзник) или красная (враг) рамка
- Иконки автоматически обновляются при смене хода

---

## 📦 Быстрая установка

### Шаг 1: Создать префаб иконки

1. **Создайте пустой GameObject** в иерархии
2. Назовите его `TurnOrderIcon`
3. Добавьте компонент `RectTransform` (должен быть автоматически)

#### Структура иконки:

```
TurnOrderIcon (RectTransform, TurnOrderIcon.cs)
├── Frame (Image) - рамка
│   └── Background (Image) - фон
│       └── CreaturePortrait (Image + Outline) - портрет существа
```

#### Настройка компонентов:

**TurnOrderIcon:**
- Размер: 100x100
- Pivot: 0.5, 0.5

**Frame (Image):**
- Размер: 100x100
- Цвет: Белый (будет меняться программно)
- Sprite: Ваш спрайт рамки

**Background (Image):**
- Размер: 95x95 (чуть меньше рамки)
- Цвет: По желанию
- Sprite: Фоновый спрайт (опционально)

**CreaturePortrait (Image + Outline):**
- Размер: 75x75
- Цвет: Белый
- Sprite: Будет задаваться программно
- **Outline компонент:**
  - Effect Distance: X=2, Y=2
  - Effect Color: Черный

### Шаг 2: Настроить компонент TurnOrderIcon

На объекте `TurnOrderIcon` добавьте скрипт `TurnOrderIcon.cs` и настройте ссылки:

- **Frame Rect**: перетащите `Frame` (RectTransform)
- **Frame Image**: перетащите `Frame` (Image)
- **Background Image**: перетащите `Background` (Image)
- **Creature Image**: перетащите `CreaturePortrait` (Image)
- **Creature Rect**: перетащите `CreaturePortrait` (RectTransform)

**Настройки цветов:**
- Active Frame Color: Yellow (255, 255, 0)
- Ally Frame Color: Green (0, 255, 0)
- Enemy Frame Color: Red (255, 0, 0)

### Шаг 3: Сохранить как префаб

1. Перетащите `TurnOrderIcon` из иерархии в папку `Assets/Prefabs/UI/`
2. Удалите из сцены (теперь это префаб)

### Шаг 4: Создать контейнер для иконок

1. В Canvas создайте пустой GameObject
2. Назовите `TurnOrderContainer`
3. **НЕ ДОБАВЛЯЙТЕ** Horizontal Layout Group вручную - скрипт сделает это автоматически
4. Настройте RectTransform:
   - Anchor: Bottom-Left (нижний левый угол)
   - Позиция: Внизу экрана, справа от кнопки Settings
   - Размер: Width = 1100, Height = 150 (примерно)

### Шаг 5: Создать менеджер

1. В сцене создайте пустой GameObject
2. Назовите `TurnOrderUI`
3. Добавьте скрипт `TurnOrderUI.cs`
4. Настройте в инспекторе:

**Зависимости:**
   - **Turn Order Controller**: перетащите объект с `TurnOrderController`
   - **Creature Manager**: перетащите объект с `CreatureManager`

**UI:**
   - **Icon Prefab**: перетащите префаб `TurnOrderIcon`
   - **Icons Container**: перетащите `TurnOrderContainer`

**Настройки расположения:**
   - **Icon Spacing**: 0 (иконки вплотную друг к другу)
   - **Left Padding**: 10 (отступ от кнопки Settings)

### Шаг 6: Готово! ✨

Запустите игру - иконки должны появиться справа от кнопки Settings и обновляться при смене хода!

---

## 🎨 Кастомизация

### Изменить размеры

В `TurnOrderIcon.cs`:
```csharp
private const float NORMAL_SIZE = 100f;              // Базовый размер рамки
private const float ACTIVE_SIZE_MULTIPLIER = 1.25f;  // Множитель для активной
private const float CREATURE_ICON_NORMAL_SIZE = 75f; // Размер портрета
```

### Изменить расстояние между иконками

В инспекторе `TurnOrderUI`:
- **Icon Spacing**: 0 = вплотную, 5 = с небольшим отступом, 10 = заметный отступ
- **Left Padding**: Отступ слева от кнопки Settings

### Изменить количество иконок

В `TurnOrderUI.cs`:
```csharp
private const int MAX_VISIBLE_ICONS = 10; // Измените на нужное число
```

### Изменить цвета

В инспекторе `TurnOrderIcon` префаба:
- Active Frame Color - цвет активной иконки (желтый)
- Ally Frame Color - цвет союзников (зеленый)
- Enemy Frame Color - цвет врагов (красный)

---

## 🔧 Как это работает

### Поток данных:

```
1. TurnOrderController.OnTurnStart (событие)
   ↓
2. TurnOrderUI.OnTurnStarted()
   ↓
3. TurnOrderUI.UpdateIcons()
   ↓
4. ClearIcons() - удаляет старые
   ↓
5. GetTurnQueueList() - получает очередь
   ↓
6. CreateIcon() для каждого существа (макс 10)
   ↓
7. TurnOrderIcon.Setup() - настраивает визуал
```

### Очередь ходов:

```
[Текущее существо] + [TurnOrderController.TurnQueue]
       ↓
[Icon 0 (Active)] [Icon 1] [Icon 2] ... [Icon 9]
   Желтая рамка    Цветные рамки (красн/зелен)
   Размер x1.25    Размер x1
```

---

## 📝 Структура файлов

```
Assets/Scripts/UI/TurnOrder/
├── TurnOrderIcon.cs     - Одна иконка
├── TurnOrderUI.cs       - Менеджер очереди
└── SETUP_GUIDE.md       - Этот файл
```

---

## 🐛 Частые проблемы

### Иконки не появляются
✅ Проверьте что `TurnOrderUI` подписан на события  
✅ Убедитесь что `iconPrefab` назначен  
✅ Проверьте что `iconsContainer` существует

### Неправильные цвета
✅ Проверьте настройки цветов в префабе `TurnOrderIcon`  
✅ Убедитесь что `frameImage` назначен

### Иконки накладываются
✅ Проверьте `Horizontal Layout Group` на контейнере  
✅ Убедитесь что у иконок правильный `RectTransform`

### Нет спрайтов существ
✅ Проверьте что у `CreatureData` заполнено поле `sprite`  
✅ Убедитесь что `creatureImage` назначен в `TurnOrderIcon`

---

## ⚙️ Опциональные улучшения

### TODO (вы можете добавить позже):

- [ ] Анимация появления/исчезновения иконок
- [ ] Плавное перемещение при смене хода
- [ ] Всплывающие подсказки с именем существа
- [ ] Индикатор здоровья на иконке
- [ ] Иконки эффектов/баффов
- [ ] Звуковые эффекты при смене хода

---

**Система готова к использованию!** 🎉

