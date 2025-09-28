using UnityEngine;

public class CreatureAnimatorController: MonoBehaviour
{
    [SerializeField] private Animator animator;

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
}