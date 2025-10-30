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

    // ========== Данные ==========
    private List<TurnOrderIcon> activeIcons = new List<TurnOrderIcon>();
    private Queue<Creature> currentTurnQueue;

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
        UpdateIcons();
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
    /// Обновляет отображаемые иконки в соответствии с текущей очередью.
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

