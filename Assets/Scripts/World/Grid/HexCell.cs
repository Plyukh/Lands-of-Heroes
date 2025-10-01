using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class HexCell : MonoBehaviour
{
    [Header("Grid Position")]
    [Tooltip("Row index in the grid")]
    public int row;
    [Tooltip("Column index in the grid")]
    public int column;

    [Header("Cell State")]
    [Tooltip("Можно ли ходить по этой клетке; обновляется автоматически при изменении occupants")]
    [SerializeField] private bool isWalkable = true;
    public bool IsWalkable => isWalkable;

    [Header("Occupants")]
    [Tooltip("Список объектов, стоящих на этой клетке")]
    [SerializeField] private List<CellOccupant> occupants = new List<CellOccupant>();
    public IReadOnlyList<CellOccupant> Occupants => occupants;

    [Header("Outline Objects")]
    [SerializeField] private Animator outlineAnimator;
    [Tooltip("Статичный контур")]
    [SerializeField] private GameObject inactiveOutline;
    [Tooltip("Активная подсветка")]
    [SerializeField] private GameObject activeOutline;
    [Tooltip("Эффект частицы для активной подсветки")]
    [SerializeField] private ParticleSystem activeOutlineEffect;

    [Header("Visual")]
    [Tooltip("Renderer для изменения материала клетки")]
    [SerializeField] private Renderer cellRenderer;

    public string CellId => $"r{row}_c{column}";

    private static readonly int EnableHash = Animator.StringToHash("Enable");
    private static readonly int DisableHash = Animator.StringToHash("Disable");
    private bool isDisabling;

    public void SetMaterial(Material mat)
    {
        if (cellRenderer != null && mat != null)
            cellRenderer.material = mat;
    }

    public void AddOccupant(GameObject instance, CellObjectType type)
    {
        if (instance == null || type == CellObjectType.None)
            return;

        if (occupants.Any(o => o.instance == instance))
            return;

        instance.transform.SetParent(transform, false);
        instance.transform.position = transform.position;

        occupants.Add(new CellOccupant
        {
            instance = instance,
            type = type
        });

        if (type == CellObjectType.Creature || type == CellObjectType.Obstacle)
            RefreshWalkable();
    }

    public void RemoveOccupant(GameObject instance)
    {
        var occ = occupants.FirstOrDefault(o => o.instance == instance);
        if (occ == null)
            return;

        occupants.Remove(occ);
        RefreshWalkable();
    }

    public void ClearAllOccupants()
    {
        occupants.Clear();
        RefreshWalkable();
    }

    public void RefreshWalkable()
    {
        var blocking = new[]
        {
            CellObjectType.Creature,
            CellObjectType.Obstacle,
            CellObjectType.ForceField
        };

        isWalkable = !occupants.Any(o => blocking.Contains(o.type));
    }

    public void ShowHighlight(bool highlight)
    {
        // Если пытаются включить, но клетка не проходима — выходим
        if (highlight && !IsWalkable)
            return;

        if (highlight)
        {
            // Включаем только активный контур
            inactiveOutline?.SetActive(false);
            isDisabling = false;
            activeOutline?.SetActive(true);

            outlineAnimator.ResetTrigger(DisableHash);
            outlineAnimator.SetTrigger(EnableHash);

            if (activeOutlineEffect != null)
            {
                var main = activeOutlineEffect.main;
                main.loop = true;
                activeOutlineEffect.Play(true);
            }
        }
        else
        {
            // Запускаем анимацию выключения
            isDisabling = true;
            outlineAnimator.ResetTrigger(EnableHash);
            outlineAnimator.SetTrigger(DisableHash);

            if (activeOutlineEffect != null)
            {
                var main = activeOutlineEffect.main;
                main.loop = false;
                activeOutlineEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            StartCoroutine(DisableAfterAnimation());
        }
    }

    private IEnumerator DisableAfterAnimation()
    {
        // Ждём фрейм, чтобы Animator успел перейти в Disabled-стейт
        yield return null;

        // Берём длину текущего клипа (должен быть Disabled)
        var info = outlineAnimator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(info.length);

        if (isDisabling)
        {
            // Скрываем оба контура сразу по окончании анимации
            activeOutline?.SetActive(false);
            inactiveOutline?.SetActive(false);
        }
    }

    public void ResetHighlight()
    {
        StopAllCoroutines();
        outlineAnimator.ResetTrigger(EnableHash);
        outlineAnimator.ResetTrigger(DisableHash);
        activeOutline?.SetActive(false);
        inactiveOutline?.SetActive(false);
        if (activeOutlineEffect != null)
            activeOutlineEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
