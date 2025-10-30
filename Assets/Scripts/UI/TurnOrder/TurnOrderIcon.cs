using System.Collections;
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
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup; // Для управления прозрачностью

    // ========== Unity Callbacks ==========
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        // Добавляем LayoutElement для возможности игнорировать Layout во время анимации
        var layoutElement = GetComponent<UnityEngine.UI.LayoutElement>();
        if (layoutElement == null)
            layoutElement = gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
    }

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
        SetBackgroundSprite();
        SetFrameColor();
        SetSize();
        SetOutline();
        
        // Устанавливаем полную непрозрачность при создании
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    // ========== Настройка визуала ==========
    private void SetCreatureIcon()
    {
        if (creature == null || creatureImage == null)
            return;

        // Получаем спрайт существа из CreatureData и устанавливаем в creatureImage
        if (creature.Data != null && creature.Data.sprite != null)
        {
            creatureImage.sprite = creature.Data.sprite;
            creatureImage.enabled = true;
        }
    }

    private void SetBackgroundSprite()
    {
        if (creature == null || backgroundImage == null)
            return;

        // Устанавливаем фоновый спрайт из CreatureData в backgroundImage
        if (creature.Data != null)
        {
            if (creature.Data.backgroundSprite != null)
            {
                backgroundImage.sprite = creature.Data.backgroundSprite;
                backgroundImage.enabled = true;
            }
            else
            {
                // Если фоновый спрайт не задан, отключаем фон
                backgroundImage.enabled = false;
            }
        }
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

    /// <summary>
    /// Проверяет, является ли эта иконка активной.
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }

    // ========== Анимация удаления ==========
    /// <summary>
    /// Запускает анимацию удаления иконки: улет влево с уменьшением прозрачности.
    /// </summary>
    /// <param name="duration">Длительность анимации в секундах</param>
    /// <param name="distance">Расстояние улета влево в пикселях</param>
    /// <param name="onComplete">Callback вызываемый после завершения анимации</param>
    public void AnimateRemove(float duration = 0.5f, float distance = 300f, System.Action onComplete = null)
    {
        if (rectTransform == null)
        {
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(RemoveAnimationCoroutine(duration, distance, onComplete));
    }

    private System.Collections.IEnumerator RemoveAnimationCoroutine(float duration, float distance, System.Action onComplete)
    {
        if (rectTransform == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        // Отключаем Layout Element, чтобы Layout Group не влиял на позицию во время анимации
        var layoutElement = GetComponent<UnityEngine.UI.LayoutElement>();
        if (layoutElement != null)
            layoutElement.ignoreLayout = true;

        Vector2 startPosition = rectTransform.anchoredPosition;
        // Улетаем влево на заданное расстояние
        Vector2 endPosition = startPosition - new Vector2(distance, 0f);
        
        // Сохраняем начальные альфа-каналы для всех Image компонентов
        float startAlphaFrame = frameImage != null ? frameImage.color.a : 1f;
        float startAlphaBackground = backgroundImage != null ? backgroundImage.color.a : 1f;
        float startAlphaCreature = creatureImage != null ? creatureImage.color.a : 1f;
        
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Движение влево
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);

            // Уменьшение прозрачности для всех элементов одновременно
            float currentAlpha = Mathf.Lerp(1f, 0f, t);
            
            // Рамка
            if (frameImage != null)
            {
                Color frameColor = frameImage.color;
                frameColor.a = currentAlpha;
                frameImage.color = frameColor;
            }
            
            // Фон
            if (backgroundImage != null)
            {
                Color bgColor = backgroundImage.color;
                bgColor.a = currentAlpha;
                backgroundImage.color = bgColor;
            }
            
            // Портрет существа
            if (creatureImage != null)
            {
                Color creatureColor = creatureImage.color;
                creatureColor.a = currentAlpha;
                creatureImage.color = creatureColor;
            }

            yield return null;
        }

        // Убеждаемся, что финальные значения установлены
        rectTransform.anchoredPosition = endPosition;
        
        // Устанавливаем полную прозрачность для всех элементов
        if (frameImage != null)
        {
            Color frameColor = frameImage.color;
            frameColor.a = 0f;
            frameImage.color = frameColor;
        }
        
        if (backgroundImage != null)
        {
            Color bgColor = backgroundImage.color;
            bgColor.a = 0f;
            backgroundImage.color = bgColor;
        }
        
        if (creatureImage != null)
        {
            Color creatureColor = creatureImage.color;
            creatureColor.a = 0f;
            creatureImage.color = creatureColor;
        }

        // Вызываем callback (объект будет уничтожен в TurnOrderUI)
        onComplete?.Invoke();
    }

    // ========== Анимация сдвига ==========
    /// <summary>
    /// Анимирует сдвиг иконки влево от текущей позиции к новой.
    /// </summary>
    /// <param name="targetPosition">Целевая позиция для сдвига</param>
    /// <param name="duration">Длительность анимации в секундах</param>
    /// <param name="animateToActive">Если true, также анимирует увеличение размера и изменение цвета на желтый</param>
    /// <param name="onComplete">Callback вызываемый после завершения анимации</param>
    public void AnimateSlide(Vector2 targetPosition, float duration = 0.3f, bool animateToActive = false, System.Action onComplete = null)
    {
        if (rectTransform == null)
        {
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(SlideAnimationCoroutine(targetPosition, duration, animateToActive, onComplete));
    }

    private System.Collections.IEnumerator SlideAnimationCoroutine(Vector2 targetPosition, float duration, bool animateToActive, System.Action onComplete)
    {
        if (rectTransform == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        // Отключаем Layout Element во время анимации
        var layoutElement = GetComponent<UnityEngine.UI.LayoutElement>();
        if (layoutElement != null)
            layoutElement.ignoreLayout = true;

        Vector2 startPosition = rectTransform.anchoredPosition;
        
        // Начальные значения для размера и цвета (если анимируем к активному состоянию)
        Vector2 startFrameSize = frameRect != null ? frameRect.sizeDelta : Vector2.zero;
        Vector2 startCreatureSize = creatureRect != null ? creatureRect.sizeDelta : Vector2.zero;
        Color startFrameColor = frameImage != null ? frameImage.color : Color.white;
        
        // Целевые значения для активного состояния
        Vector2 targetFrameSize = startFrameSize * ACTIVE_SIZE_MULTIPLIER;
        Vector2 targetCreatureSize = startCreatureSize * ACTIVE_SIZE_MULTIPLIER;
        Color targetFrameColor = activeFrameColor;
        
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            // Используем плавную кривую для более естественного движения
            t = t * t * (3f - 2f * t); // SmoothStep для плавности

            // Сдвиг влево
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);

            // Если анимируем к активному состоянию, меняем размер и цвет
            if (animateToActive)
            {
                // Анимация размера рамки
                if (frameRect != null)
                {
                    frameRect.sizeDelta = Vector2.Lerp(startFrameSize, targetFrameSize, t);
                }
                
                // Анимация размера портрета существа
                if (creatureRect != null)
                {
                    creatureRect.sizeDelta = Vector2.Lerp(startCreatureSize, targetCreatureSize, t);
                }
                
                // Анимация цвета рамки
                if (frameImage != null)
                {
                    frameImage.color = Color.Lerp(startFrameColor, targetFrameColor, t);
                }
            }

            yield return null;
        }

        // Убеждаемся, что финальные значения установлены
        rectTransform.anchoredPosition = targetPosition;
        
        // Устанавливаем финальные размеры и цвет для активного состояния
        if (animateToActive)
        {
            if (frameRect != null)
                frameRect.sizeDelta = targetFrameSize;
            if (creatureRect != null)
                creatureRect.sizeDelta = targetCreatureSize;
            if (frameImage != null)
                frameImage.color = targetFrameColor;
            
            // Обновляем статус
            isActive = true;
        }

        // Включаем Layout Element обратно
        if (layoutElement != null)
            layoutElement.ignoreLayout = false;

        // Вызываем callback
        onComplete?.Invoke();
    }
}

