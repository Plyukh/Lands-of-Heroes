using System;
using UnityEngine;

public class CreatureAnimatorController: MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Projectile Settings")]
    [Tooltip("Откуда должен появляться снаряд при стрельбе")]
    [SerializeField] private Transform projectileSpawnPoint;

    private Creature attackerCreature;
    private Creature currentTarget;

    public event Action OnAttackHit;
    public event Action OnMeleeAttackHit;

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
        if (prefab == null) return;

        var projObj = Instantiate(prefab,
                                  projectileSpawnPoint.position,
                                  projectileSpawnPoint.rotation);
        var proj = projObj.GetComponent<ProjectileController>();
        if (proj == null) return;

        proj.Initialize(
            currentTarget.transform,
            () => currentTarget.Mover.AnimatorController.PlayImpact());
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

        currentTarget.Mover.AnimatorController.PlayImpact();
    }

    public void RotateToAttackerEvent()
    {
        currentTarget.Mover.RotateTowardsAsync(
            attackerCreature.transform.position);
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
        animator.SetTrigger("DoubleShot");
    }

    public void PlayDoubleBlow()
    {
        animator.SetTrigger("DoubleBlow"); 
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