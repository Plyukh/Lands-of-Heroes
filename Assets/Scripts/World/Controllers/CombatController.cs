using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public class CombatController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private HighlightController highlightController;
    [SerializeField] private HexGridManager hexGridManager;

    public event Action<Creature> OnCombatComplete;

    /// <summary>
    /// Вызывается, когда нужно провести атаку: заранее 
    /// персонаж уже переместился в нужную клетку.
    /// </summary>
    public async void OnCreatureClicked(Creature attacker, Creature target, AttackType selectedType)
    {
        if (attacker == null || target == null || attacker == target)
            return;
        if (!TurnOrderController.Instance.IsCurrentTurn(attacker))
            return;

        // Убираем все подсветки перед атакой
        highlightController.ClearHighlights();

        // Поворачиваемся лицом к цели
        await attacker.Mover.RotateTowardsAsync(target.transform.position);

        // Запускаем последовательность атак (с контратаками между ударами)
        await PlayAttackSequence(attacker, target, selectedType);

        // Проверяем специальные эффекты ударов (после всех атак и контратак)
        if (attacker.EffectManager.HasEffectOfType(EffectType.AreaStrike))
        {
            PerformAreaStrike(attacker, target);
        }
        
        if (attacker.EffectManager.HasEffectOfType(EffectType.PiercingStrike))
        {
            PerformPiercingStrike(attacker, target);
        }

        // Оповещаем, что атака завершилась
        OnCombatComplete?.Invoke(attacker);
    }

    private async Task PlayAttackSequence(Creature attacker, Creature target, AttackType type)
    {
        var anim = attacker.Mover.AnimatorController;
        
        // Проверяем, есть ли эффект двойной атаки
        bool hasDoubleAttack = false;
        if (type == AttackType.Ranged)
        {
            hasDoubleAttack = attacker.EffectManager.HasEffectOfType(EffectType.DoubleShot);
        }
        else
        {
            hasDoubleAttack = attacker.EffectManager.HasEffectOfType(EffectType.DoubleBlow);
        }

        // Устанавливаем цель анимации
        anim.SetAttackTarget(target, attacker);

        // Количество атак (1 или 2)
        int attackCount = hasDoubleAttack ? 2 : 1;

        // Выполняем атаку нужное количество раз
        for (int i = 0; i < attackCount; i++)
        {
            // Выполняем атаку
            await PlaySingleAttack(attacker, type);

            // После каждого удара - контратака (только для ближнего боя)
            if (type == AttackType.Melee)
            {
                await PerformCounterattack(attacker, target);
            }
        }
    }

    /// <summary>
    /// Выполняет одну атаку и ждет её завершения
    /// </summary>
    private async Task PlaySingleAttack(Creature attacker, AttackType type)
    {
        var anim = attacker.Mover.AnimatorController;
        var tcs = new TaskCompletionSource<bool>();

        Action onHit = null;
        if (type == AttackType.Ranged)
        {
            onHit = () =>
            {
                anim.OnAttackHit -= onHit;
                tcs.TrySetResult(true);
            };
            anim.OnAttackHit += onHit;
            anim.PlayAttack();
        }
        else
        {
            // Для ближнего боя дальников используем OnAttackHit, так как HandleAttackHitEvent 
            // всегда вызывает OnAttackHit для дальников независимо от типа атаки
            if (attacker.AttackType == AttackType.Ranged)
            {
                onHit = () =>
                {
                    anim.OnAttackHit -= onHit;
                    tcs.TrySetResult(true);
                };
                anim.OnAttackHit += onHit;
            }
            else
            {
                onHit = () =>
                {
                    anim.OnMeleeAttackHit -= onHit;
                    tcs.TrySetResult(true);
                };
                anim.OnMeleeAttackHit += onHit;
            }
            anim.PlayMeleeAttack();
        }

        await tcs.Task;
    }

    /// <summary>
    /// Выполняет атаку по площади - все враги вокруг атакующего получают урон
    /// </summary>
    private void PerformAreaStrike(Creature attacker, Creature mainTarget)
    {
        var attackerCell = attacker.Mover.CurrentCell;
        if (attackerCell == null)
            return;

        // Находим всех врагов в соседних клетках
        var neighbors = hexGridManager.GetNeighbors(attackerCell);
        var enemiesAround = new System.Collections.Generic.List<Creature>();

        foreach (var neighborCell in neighbors)
        {
            var creature = neighborCell.GetOccupantCreature();
            
            // Проверяем: существо есть, это враг, и это не главная цель (её мы уже атаковали)
            if (creature != null && 
                creature.Side != attacker.Side && 
                creature != mainTarget)
            {
                enemiesAround.Add(creature);
            }
        }

        // Все враги вокруг получают урон (без поворота)
        foreach (var enemy in enemiesAround)
        {
            // Враг защищается или получает удар
            if (enemy.IsDefending)
            {
                enemy.Mover.AnimatorController.PlayBlock();
                enemy.Mover.AnimatorController.PlayBlockImpact();
            }
            else
            {
                enemy.Mover.AnimatorController.PlayImpact();
            }
        }
    }

    /// <summary>
    /// Выполняет пробивающий удар - атакует существо ЗА целью (в том же направлении)
    /// </summary>
    private void PerformPiercingStrike(Creature attacker, Creature mainTarget)
    {
        var attackerCell = attacker.Mover.CurrentCell;
        var targetCell = mainTarget.Mover.CurrentCell;
        
        if (attackerCell == null || targetCell == null)
            return;

        // Находим направление через соседей цели
        var targetNeighbors = hexGridManager.GetNeighbors(targetCell).ToList();
        var attackerNeighbors = hexGridManager.GetNeighbors(attackerCell).ToList();
        
        // Ищем в каком направлении от атакующего находится цель
        int directionIndex = attackerNeighbors.IndexOf(targetCell);
        
        HexCell behindCell = null;
        
        if (directionIndex != -1)
        {
            // Цель - соседняя клетка, используем тот же индекс для поиска клетки за целью
            if (directionIndex < targetNeighbors.Count)
            {
                behindCell = targetNeighbors[directionIndex];
            }
        }
        else
        {
            // Цель не соседняя - используем позиционный вектор
            // Находим ближайшего соседа атакующего в направлении цели
            float minAngle = float.MaxValue;
            int bestDirIndex = -1;
            
            UnityEngine.Vector3 targetDir = (targetCell.transform.position - attackerCell.transform.position).normalized;
            
            for (int i = 0; i < attackerNeighbors.Count; i++)
            {
                UnityEngine.Vector3 neighborDir = (attackerNeighbors[i].transform.position - attackerCell.transform.position).normalized;
                float angle = UnityEngine.Vector3.Angle(targetDir, neighborDir);
                
                if (angle < minAngle)
                {
                    minAngle = angle;
                    bestDirIndex = i;
                }
            }
            
            // Используем найденное направление для поиска клетки за целью
            if (bestDirIndex != -1 && bestDirIndex < targetNeighbors.Count)
            {
                behindCell = targetNeighbors[bestDirIndex];
            }
        }

        // Если нашли клетку ЗА целью
        if (behindCell != null)
        {
            var creature = behindCell.GetOccupantCreature();
            
            // Проверяем: существо есть и это враг
            if (creature != null && creature.Side != attacker.Side)
            {
                // Существо ЗА целью получает урон
                if (creature.IsDefending)
                {
                    creature.Mover.AnimatorController.PlayBlock();
                    creature.Mover.AnimatorController.PlayBlockImpact();
                }
                else
                {
                    creature.Mover.AnimatorController.PlayImpact();
                }
            }
        }
    }

    /// <summary>
    /// Выполняет контратаку - цель отвечает на атаку ближнего боя
    /// Counterattack = количество АТАК, на которые можно ответить (не количество ударов)
    /// ВАЖНО: Контратака - это простой одиночный удар БЕЗ контратаки в ответ!
    /// </summary>
    private async Task PerformCounterattack(Creature attacker, Creature defender)
    {
        // Если у атакующего есть Unanswered Strike - контратак не будет
        if (attacker.EffectManager.HasEffectOfType(EffectType.UnansweredStrike))
            return;

        // Проверяем, может ли защищающийся контратаковать эту атаку
        if (!defender.UseCounterattack())
            return;

        // Защищающийся поворачивается к атакующему
        await defender.Mover.RotateTowardsAsync(attacker.transform.position);

        // Защищающийся контратакует ОДИН РАЗ - простая одиночная атака
        // БЕЗ рекурсивного вызова контратак!
        var anim = defender.Mover.AnimatorController;
        anim.SetAttackTarget(attacker, defender);
        await PlaySingleAttack(defender, defender.AttackType);
    }
}
