using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Одна иконка в очереди ходов.
/// Отображает портрет существа, рамку нужного цвета и размер в зависимости от позиции.
/// </summary>
public class TurnOrderIcon : MonoBehaviour
{
    // ========== Константы ==========
    private const float NORMAL_SIZE = 100f;
    private const float ACTIVE_SIZE_MULTIPLIER = 1.25f;
    private const float CREATURE_ICON_NORMAL_SIZE = 75f;

    // ========== Компоненты ==========
    [Header("UI Компоненты")]
    [SerializeField] 
    [Tooltip("Рамка иконки")]
    private RectTransform frameRect;
    
    [SerializeField] 
    [Tooltip("Image компонент рамки для цвета")]
    private Image frameImage;
    
    [SerializeField] 
    [Tooltip("Фоновый Image внутри иконки")]
    private Image backgroundImage;
    
    [SerializeField] 
    [Tooltip("Image с портретом существа")]
    private Image creatureImage;
    
    [SerializeField] 
    [Tooltip("RectTransform для портрета существа")]
    private RectTransform creatureRect;

    [Header("Настройки цветов")]
    [SerializeField] 
    [Tooltip("Цвет рамки для активного существа")]
    private Color activeFrameColor = Color.yellow;
    
    [SerializeField] 
    [Tooltip("Цвет рамки для союзного существа")]
    private Color allyFrameColor = Color.green;
    
    [SerializeField] 
    [Tooltip("Цвет рамки для вражеского существа")]
    private Color enemyFrameColor = Color.red;

    // ========== Данные ==========
    private Creature creature;
    private bool isActive;

    // ========== Инициализация ==========
    /// <summary>
    /// Настраивает иконку для конкретного существа.
    /// </summary>
    /// <param name="creature">Существо для отображения</param>
    /// <param name="isActive">Является ли это существо активным (ходит сейчас)</param>
    public void Setup(Creature creature, bool isActive)
    {
        this.creature = creature;
        this.isActive = isActive;

        SetCreatureIcon();
        SetFrameColor();
        SetSize();
        SetOutline();
    }

    // ========== Настройка визуала ==========
    private void SetCreatureIcon()
    {
        if (creature == null || creatureImage == null)
            return;

        // Получаем спрайт из CreatureData
        if (creature.Data != null && creature.Data.sprite != null)
            creatureImage.sprite = creature.Data.sprite;
    }

    private void SetFrameColor()
    {
        if (frameImage == null)
            return;

        // Активное существо - желтая рамка
        if (isActive)
        {
            frameImage.color = activeFrameColor;
            return;
        }

        if(creature.Side == TargetSide.Ally)
        {
            frameImage.color = allyFrameColor;
        }
        else
        {
            frameImage.color = enemyFrameColor;
        }
    }

    private void SetSize()
    {
        float frameSize = NORMAL_SIZE;
        float creatureSize = CREATURE_ICON_NORMAL_SIZE;

        // Активное существо больше
        if (isActive)
        {
            frameSize *= ACTIVE_SIZE_MULTIPLIER;
            creatureSize *= ACTIVE_SIZE_MULTIPLIER;
        }

        // Применяем размеры
        if (frameRect != null)
            frameRect.sizeDelta = new Vector2(frameSize, frameSize);

        if (creatureRect != null)
            creatureRect.sizeDelta = new Vector2(creatureSize, creatureSize);
    }

    private void SetOutline()
    {
        if (creatureImage == null)
            return;

        // Получаем или добавляем компонент Outline
        var outline = creatureImage.GetComponent<Outline>();
        if (outline == null)
            outline = creatureImage.gameObject.AddComponent<Outline>();

        // Настраиваем Outline (пока стандартные значения)
        outline.effectDistance = new Vector2(2f, 2f);
        outline.effectColor = Color.black;
    }

    // ========== Публичные методы ==========
    /// <summary>
    /// Обновляет статус иконки (активна или нет).
    /// </summary>
    public void UpdateActiveStatus(bool newIsActive)
    {
        isActive = newIsActive;
        SetFrameColor();
        SetSize();
    }

    /// <summary>
    /// Получить существо, которое отображается этой иконкой.
    /// </summary>
    public Creature GetCreature()
    {
        return creature;
    }
}

