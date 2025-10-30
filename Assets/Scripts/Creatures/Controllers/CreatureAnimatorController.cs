using System;
using UnityEngine;

public class CreatureAnimatorController: MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Projectile Settings")]
    [Tooltip("������ ������ ���������� ������ ��� ��������")]
    [SerializeField] private Transform projectileSpawnPoint;

    private Creature attackerCreature;
    private Creature currentTarget;
    
    // Множитель скорости анимаций
    private int animationSpeedMultiplier = 1;

    public event Action OnAttackHit;
    public event Action OnMeleeAttackHit;

    /// <summary>
    /// Устанавливает множитель скорости для всех анимаций
    /// </summary>
    public void SetAnimationSpeed(int multiplier)
    {
        animationSpeedMultiplier = Mathf.Clamp(multiplier, 1, 3);
        
        if (animator != null)
        {
            animator.speed = animationSpeedMultiplier;
        }
    }

    public void SetAttackTarget(Creature target, Creature attacker)
    {
        currentTarget = target;
        attackerCreature = attacker;
    }

    public void SpawnProjectileEvent()
    {
        if (attackerCreature == null || currentTarget == null || projectileSpawnPoint == null)
            return;

        var prefab = attackerCreature.Projectile;
        if (prefab == null)
            return;

        var projObj = Instantiate(
            prefab,
            projectileSpawnPoint.position,
            projectileSpawnPoint.rotation);

        var proj = projObj.GetComponent<ProjectileController>();
        if (proj == null)
            return;

        // 0f � � ����; 0.5f � � �����; 1f � � ������
        const float heightNormalized = 1f;

        // ������ callback � ������ � ����.
        Action onHit = null;
        if (!currentTarget.IsDefending)
        {
            onHit = () => currentTarget?.Mover?.AnimatorController?.PlayImpact();
        }
        else
        {
            onHit = () => currentTarget?.Mover?.AnimatorController?.PlayBlockImpact();
        }

        // ������ callback � ����������� � ���������� ���������,
        // ����� ��������� �������� �������� ������� OnAttackHit � ��� ����������.
        Action onComplete = () => attackerCreature?.Mover?.AnimatorController?.HandleAttackHit();

        proj.Initialize(currentTarget.transform, heightNormalized, onHit, onComplete);
    }

    public void HandleAttackHitEvent()
    {
        if (attackerCreature == null || currentTarget == null)
            return;

        bool isRanged = attackerCreature.AttackType == AttackType.Ranged;

        if (isRanged)
            OnAttackHit?.Invoke();
        else
            OnMeleeAttackHit?.Invoke();

        if (currentTarget.IsDefending)
        {
            currentTarget.Mover.AnimatorController.PlayBlockImpact();
        }
        else
        {
            currentTarget.Mover.AnimatorController.PlayImpact();
        }
    }

    public void RotateToAttackerEvent()
    {
        currentTarget.Mover.RotateTowardsAsync(
            attackerCreature.transform.position);
        if (currentTarget.IsDefending)
        {
            currentTarget.Mover.AnimatorController.PlayBlock();
        }
    }

    public void PlayWalk(bool isWalking)
    {
        animator.SetBool("IsWalking", isWalking);
    }

    public void PlayStartTeleport()
    {
        animator.SetTrigger("StartTeleport");
    }

    public void PlayEndTeleport()
    {
        animator.SetTrigger("EndTeleport");
    }

    public void PlayAttack()
    {
        animator.SetTrigger("Attack");
    }

    public void PlayMeleeAttack()
    {
        animator.SetTrigger("MeleeAttack");
    }

    public void PlayDoubleShot()
    {
        animator.SetBool("DoubleShot", true);
    }

    public void PlayDoubleBlow()
    {
        animator.SetBool("DoubleBlow", true);
    }

    public void PlayImpact()
    {
        animator.SetTrigger("Impact");
    }

    public void PlayDeath()
    {
        animator.SetTrigger("Death");
    }

    public void PlayCast()
    {
        animator.SetTrigger("Cast");
    }

    public void PlayBlock()
    {
        animator.SetTrigger("Block");
    }

    public void PlayBlockImpact()
    {
        animator.SetTrigger("BlockImpact");
    }

    public void HandleAttackHit()
    {
        OnAttackHit?.Invoke();
    }

    public void HandleMeleeAttackHit()
    {
        OnMeleeAttackHit?.Invoke();
    }
}