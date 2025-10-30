# UI Очереди ходов - Быстрая настройка

## ⚡ За 5 минут

### 1. Создать префаб иконки

**Структура:**
```
TurnOrderIcon (100x100)
├── Frame (Image, 100x100) - рамка
│   └── Background (Image, 95x95) - фон
│       └── CreaturePortrait (Image + Outline, 75x75) - портрет
```

**Компонент TurnOrderIcon.cs:**
- Frame Rect → Frame (RectTransform)
- Frame Image → Frame (Image)
- Background Image → Background (Image)
- Creature Image → CreaturePortrait (Image)
- Creature Rect → CreaturePortrait (RectTransform)

**Цвета:**
- Active: Yellow (255, 255, 0)
- Ally: Green (0, 255, 0)
- Enemy: Red (255, 0, 0)

**Сохранить в:** `Assets/Prefabs/UI/TurnOrderIcon.prefab`

---

### 2. Создать контейнер

**В Canvas:**
- GameObject → `TurnOrderContainer`
- Anchor: Bottom-Left
- Позиция: Справа от кнопки Settings
- **НЕ добавляйте** Layout Group (скрипт сделает сам)

---

### 3. Настроить TurnOrderUI

**Создать:** GameObject → `TurnOrderUI`

**Добавить:** `TurnOrderUI.cs`

**Настроить:**
- Turn Order Controller → найти в сцене
- Creature Manager → найти в сцене
- Icon Prefab → `TurnOrderIcon` префаб
- Icons Container → `TurnOrderContainer`
- Icon Spacing → **0** (вплотную)
- Left Padding → **10** (отступ от Settings)

---

### 4. Запустить ✨

Готово! Иконки появятся справа от Settings, вплотную друг к другу.

---

## 🎛️ Настройки

| Параметр | Значение | Описание |
|----------|----------|----------|
| Icon Spacing | 0 | Иконки впритык |
| Left Padding | 10 | Отступ от Settings |
| MAX_VISIBLE_ICONS | 10 | Максимум иконок |

---

## 🎨 Визуал

```
[⚙️ Settings] [👤🟡][👤🟢][👤🔴][👤🟢][👤🔴]...
               ↑      ↑
           Активная  Остальные
           (желтая)  (цветные)
           x1.25     x1
```

**Размеры:**
- Активная: 125x125 (рамка), ~94x94 (портрет)
- Обычная: 100x100 (рамка), 75x75 (портрет)

---

Полная инструкция: `SETUP_GUIDE.md`

