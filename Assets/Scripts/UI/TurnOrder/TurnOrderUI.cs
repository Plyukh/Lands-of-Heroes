using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Управляет отображением очереди ходов существ в UI.
/// Создает/удаляет иконки, обновляет их порядок и статус.
/// </summary>
public class TurnOrderUI : MonoBehaviour
{
    // ========== Константы ==========
    private const int MAX_VISIBLE_ICONS = 10;

    // ========== Настройки ==========
    [Header("Зависимости")]
    [SerializeField]
    [Tooltip("Контроллер очереди ходов")]
    private TurnOrderController turnOrderController;

    [SerializeField]
    [Tooltip("Менеджер существ")]
    private CreatureManager creatureManager;

    [Header("UI")]
    [SerializeField]
    [Tooltip("Префаб иконки существа")]
    private GameObject iconPrefab;

    [SerializeField]
    [Tooltip("Контейнер для иконок")]
    private Transform iconsContainer;

    [Header("Настройки расположения")]
    [SerializeField]
    [Tooltip("Расстояние между иконками (0 = вплотную)")]
    private float iconSpacing = 0f;
    
    [SerializeField]
    [Tooltip("Отступ слева от кнопки Settings")]
    private float leftPadding = 10f;

    [Header("Настройки анимации")]
    [SerializeField]
    [Tooltip("Длительность анимации удаления активной иконки (секунды)")]
    private float removeAnimationDuration = 0.5f;
    
    [SerializeField]
    [Tooltip("Расстояние, на которое улетает иконка влево (в пикселях)")]
    private float removeAnimationDistance = 300f;
    
    [SerializeField]
    [Tooltip("Длительность анимации сдвига остальных иконок (секунды)")]
    private float slideAnimationDuration = 0.3f;

    // ========== Данные ==========
    private List<TurnOrderIcon> activeIcons = new List<TurnOrderIcon>();
    private Queue<Creature> currentTurnQueue;
    private bool isInitialized = false; // Флаг инициализации через DelayedInitialUpdate

    // ========== Unity Callbacks ==========
    private void Start()
    {
        SetupLayout();
        SubscribeToEvents();
        // Сразу обновляем UI при старте, чтобы отобразить очередь с первого хода
        // Используем корутину с небольшой задержкой, чтобы TurnOrderController успел инициализироваться
        StartCoroutine(DelayedInitialUpdate());
    }

    private System.Collections.IEnumerator DelayedInitialUpdate()
    {
        // Ждем один кадр, чтобы все Start() методы успели выполниться
        yield return null;
        UpdateIcons();
        isInitialized = true; // Отмечаем, что начальная инициализация завершена
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    // ========== Настройка Layout ==========
    /// <summary>
    /// Настраивает Horizontal Layout Group для правильного расположения иконок.
    /// </summary>
    private void SetupLayout()
    {
        if (iconsContainer == null)
            return;

        // Получаем или добавляем Horizontal Layout Group
        var layoutGroup = iconsContainer.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        if (layoutGroup == null)
            layoutGroup = iconsContainer.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();

        // Настраиваем параметры
        layoutGroup.spacing = iconSpacing;
        layoutGroup.childAlignment = UnityEngine.TextAnchor.LowerLeft; // Выравнивание по нижнему краю
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childScaleWidth = false;
        layoutGroup.childScaleHeight = false;
        
        // Настраиваем padding (отступы)
        layoutGroup.padding = new UnityEngine.RectOffset(
            (int)leftPadding,  // left
            0,                  // right
            0,                  // top
            0                   // bottom
        );
    }

    // ========== Подписка на события ==========
    private void SubscribeToEvents()
    {
        if (turnOrderController != null)
            turnOrderController.OnTurnStart += OnTurnStarted;
    }

    private void UnsubscribeFromEvents()
    {
        if (turnOrderController != null)
            turnOrderController.OnTurnStart -= OnTurnStarted;
    }

    // ========== Обработчики событий ==========
    /// <summary>
    /// Вызывается когда начинается ход нового существа.
    /// </summary>
    private void OnTurnStarted(Creature creature)
    {
        // Если еще не завершена начальная инициализация - просто обновляем иконки
        // (DelayedInitialUpdate создаст их сам)
        if (!isInitialized)
        {
            return;
        }

        // Проверяем, есть ли уже активные иконки для анимации
        bool hasActiveIcons = activeIcons.Count > 0 && HasActiveIcon();
        
        // Если активных иконок нет - создаем иконки без анимации
        if (!hasActiveIcons)
        {
            UpdateIcons();
            return;
        }

        // Если активные иконки есть - запускаем анимацию удаления
        RemoveActiveIconWithAnimation();
    }

    /// <summary>
    /// Проверяет, есть ли активная иконка в списке.
    /// </summary>
    private bool HasActiveIcon()
    {
        foreach (var icon in activeIcons)
        {
            if (icon != null && icon.IsActive())
                return true;
        }
        return false;
    }

    // ========== Обновление очереди ==========
    /// <summary>
    /// Получает текущую очередь существ из TurnOrderController.
    /// </summary>
    private List<Creature> GetTurnQueueList()
    {
        List<Creature> queueList = new List<Creature>();
        
        // Добавляем текущее существо первым
        if (turnOrderController.CurrentCreature != null)
            queueList.Add(turnOrderController.CurrentCreature);
        
        // Добавляем остальных из очереди
        if (turnOrderController.TurnQueue != null)
        {
            foreach (var creature in turnOrderController.TurnQueue)
            {
                queueList.Add(creature);
            }
        }
        
        return queueList;
    }

    /// <summary>
    /// Удаляет активную иконку с анимацией, затем обновляет очередь.
    /// </summary>
    private void RemoveActiveIconWithAnimation()
    {
        // Находим активную иконку (первая в списке)
        TurnOrderIcon activeIcon = null;
        foreach (var icon in activeIcons)
        {
            if (icon != null && icon.IsActive())
            {
                activeIcon = icon;
                break;
            }
        }

        if (activeIcon == null)
        {
            // Если активной иконки нет, просто обновляем очередь
            UpdateIcons();
            return;
        }

        // Отключаем Layout Group, чтобы остальные иконки не сдвигались во время анимации
        var layoutGroup = iconsContainer.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        bool layoutWasEnabled = false;
        if (layoutGroup != null)
        {
            layoutWasEnabled = layoutGroup.enabled;
            layoutGroup.enabled = false;
        }

        // Снимаем флаг активности у всех остальных иконок
        foreach (var icon in activeIcons)
        {
            if (icon != null && icon != activeIcon)
            {
                icon.UpdateActiveStatus(false);
            }
        }

        // Сохраняем текущие позиции остальных иконок
        Dictionary<TurnOrderIcon, Vector2> iconPositions = new Dictionary<TurnOrderIcon, Vector2>();
        foreach (var icon in activeIcons)
        {
            if (icon != null && icon != activeIcon)
            {
                var rt = icon.GetComponent<RectTransform>();
                if (rt != null)
                {
                    iconPositions[icon] = rt.anchoredPosition;
                }
            }
        }

        // Запускаем анимацию удаления
        activeIcon.AnimateRemove(removeAnimationDuration, removeAnimationDistance, () =>
        {
            // После завершения анимации удаляем иконку
            if (activeIcon != null)
            {
                activeIcons.Remove(activeIcon);
                Destroy(activeIcon.gameObject);
            }
            
            // Включаем Layout Group обратно, чтобы получить новые позиции для остальных иконок
            if (layoutGroup != null && layoutWasEnabled)
            {
                layoutGroup.enabled = true;
                // Принудительно пересчитываем Layout, чтобы получить новые позиции
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(iconsContainer as RectTransform);
            }
            
            // Анимируем сдвиг остальных иконок к новым позициям
            AnimateRemainingIconsSlide(iconPositions);
        });
    }

    /// <summary>
    /// Анимирует сдвиг остальных иконок от старых позиций к новым.
    /// </summary>
    private void AnimateRemainingIconsSlide(Dictionary<TurnOrderIcon, Vector2> oldPositions)
    {
        if (oldPositions.Count == 0)
        {
            // Если позиций нет, просто обновляем очередь
            RefreshQueueIcons();
            return;
        }

        // Временно отключаем Layout, чтобы анимировать вручную
        var layoutGroup = iconsContainer.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        if (layoutGroup != null)
            layoutGroup.enabled = false;

        // Список иконок для анимации
        List<TurnOrderIcon> iconsToAnimate = new List<TurnOrderIcon>();
        
        foreach (var icon in activeIcons)
        {
            if (icon != null && oldPositions.ContainsKey(icon))
            {
                iconsToAnimate.Add(icon);
            }
        }

        if (iconsToAnimate.Count == 0)
        {
            // Если нечего анимировать, включаем Layout обратно и обновляем очередь
            if (layoutGroup != null)
                layoutGroup.enabled = true;
            RefreshQueueIcons();
            return;
        }

        // Определяем, какая иконка станет активной (первая в списке после сдвига)
        TurnOrderIcon nextActiveIcon = iconsToAnimate.Count > 0 ? iconsToAnimate[0] : null;

        // Подсчитываем количество завершенных анимаций
        int completedAnimations = 0;
        int totalAnimations = iconsToAnimate.Count;

        // Запускаем анимацию для каждой иконки
        for (int i = 0; i < iconsToAnimate.Count; i++)
        {
            var icon = iconsToAnimate[i];
            if (icon == null || !oldPositions.ContainsKey(icon))
                continue;

            Vector2 oldPos = oldPositions[icon];
            Vector2 newPos = icon.GetComponent<RectTransform>().anchoredPosition;

            // Если позиция не изменилась, пропускаем анимацию
            if (Vector2.Distance(oldPos, newPos) < 0.1f)
            {
                completedAnimations++;
                if (completedAnimations >= totalAnimations)
                {
                    // Все анимации завершены
                    if (layoutGroup != null)
                        layoutGroup.enabled = true;
                    RefreshQueueIcons();
                }
                continue;
            }

            // Устанавливаем старую позицию для начала анимации
            icon.GetComponent<RectTransform>().anchoredPosition = oldPos;

            // Первая иконка (которая станет активной) должна также анимировать размер и цвет
            bool shouldAnimateToActive = (icon == nextActiveIcon);

            // Запускаем анимацию сдвига
            icon.AnimateSlide(newPos, slideAnimationDuration, shouldAnimateToActive, () =>
            {
                completedAnimations++;
                // Когда все анимации завершены, включаем Layout обратно и обновляем очередь
                if (completedAnimations >= totalAnimations)
                {
                    if (layoutGroup != null)
                        layoutGroup.enabled = true;
                    RefreshQueueIcons();
                }
            });
        }
    }

    /// <summary>
    /// Обновляет отображаемые иконки в соответствии с текущей очередью.
    /// Полностью пересоздает все иконки.
    /// </summary>
    private void UpdateIcons()
    {
        // Удаляем все старые иконки
        ClearIcons();

        // Получаем список существ в очереди
        List<Creature> creaturesInQueue = GetTurnQueueList();
        
        if (creaturesInQueue == null || creaturesInQueue.Count == 0)
            return;

        // Создаем новые иконки (макс 10 штук)
        int iconsToCreate = Mathf.Min(MAX_VISIBLE_ICONS, creaturesInQueue.Count);

        for (int i = 0; i < iconsToCreate; i++)
        {
            Creature creature = creaturesInQueue[i];
            bool isActive = (i == 0); // Первая иконка - активная
            
            CreateIcon(creature, isActive);
        }
    }

    /// <summary>
    /// Обновляет очередь иконок без полного пересоздания.
    /// Обновляет существующие иконки и добавляет недостающие.
    /// </summary>
    private void RefreshQueueIcons()
    {
        // Получаем список существ в очереди
        List<Creature> creaturesInQueue = GetTurnQueueList();
        
        if (creaturesInQueue == null || creaturesInQueue.Count == 0)
            return;

        // Обновляем существующие иконки
        for (int i = 0; i < activeIcons.Count && i < creaturesInQueue.Count; i++)
        {
            if (activeIcons[i] != null)
            {
                Creature newCreature = creaturesInQueue[i];
                bool isActive = (i == 0);
                
                // Если существо изменилось, пересоздаем иконку
                if (activeIcons[i].GetCreature() != newCreature)
                {
                    Destroy(activeIcons[i].gameObject);
                    activeIcons[i] = null;
                }
                else
                {
                    // Иначе просто обновляем статус
                    activeIcons[i].UpdateActiveStatus(isActive);
                }
            }
        }

        // Удаляем null-элементы из списка
        activeIcons.RemoveAll(icon => icon == null);

        // Добавляем недостающие иконки
        int maxIcons = Mathf.Min(MAX_VISIBLE_ICONS, creaturesInQueue.Count);
        while (activeIcons.Count < maxIcons)
        {
            int index = activeIcons.Count;
            Creature creature = creaturesInQueue[index];
            bool isActive = (index == 0);
            
            CreateIcon(creature, isActive);
        }

        // Удаляем лишние иконки (если очередь уменьшилась)
        while (activeIcons.Count > maxIcons)
        {
            var iconToRemove = activeIcons[activeIcons.Count - 1];
            activeIcons.RemoveAt(activeIcons.Count - 1);
            if (iconToRemove != null)
                Destroy(iconToRemove.gameObject);
        }
    }

    // ========== Создание/Удаление иконок ==========
    /// <summary>
    /// Создает одну иконку для существа.
    /// </summary>
    private void CreateIcon(Creature creature, bool isActive)
    {
        // Создаем объект из префаба
        GameObject iconObj = Instantiate(iconPrefab, iconsContainer);
        
        // Получаем компонент TurnOrderIcon
        TurnOrderIcon icon = iconObj.GetComponent<TurnOrderIcon>();
        
        if (icon == null)
        {
            Debug.LogError("TurnOrderUI: Префаб иконки не содержит компонент TurnOrderIcon!");
            Destroy(iconObj);
            return;
        }

        // Настраиваем иконку
        icon.Setup(creature, isActive);
        
        // Добавляем в список активных
        activeIcons.Add(icon);
    }

    /// <summary>
    /// Удаляет все иконки.
    /// </summary>
    private void ClearIcons()
    {
        foreach (var icon in activeIcons)
        {
            if (icon != null)
                Destroy(icon.gameObject);
        }
        
        activeIcons.Clear();
    }

    // ========== Публичные методы ==========
    /// <summary>
    /// Принудительное обновление UI очереди.
    /// Можно вызвать вручную если нужно.
    /// </summary>
    public void ForceRefresh()
    {
        UpdateIcons();
    }

    /// <summary>
    /// Обновляет настройки расположения (spacing и padding).
    /// Вызывается автоматически при изменении в инспекторе.
    /// </summary>
    private void OnValidate()
    {
        if (Application.isPlaying && iconsContainer != null)
        {
            SetupLayout();
        }
    }
}

