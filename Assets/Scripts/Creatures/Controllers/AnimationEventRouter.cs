using UnityEngine;

public class AnimationEventRouter : MonoBehaviour
{
    [SerializeField] private CreatureAnimatorController creatureAnimatorController;

    public void SpawnProjectileEvent()
    {
        if (creatureAnimatorController != null) creatureAnimatorController.SpawnProjectileEvent();
    }

    public void HandleAttackHitEvent()
    {
        if (creatureAnimatorController != null) creatureAnimatorController.HandleAttackHitEvent();
    }

    public void RotateToAttackerEvent()
    {
        if (creatureAnimatorController != null) creatureAnimatorController.RotateToAttackerEvent();
    }
}
