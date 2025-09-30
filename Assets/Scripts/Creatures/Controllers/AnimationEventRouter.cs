using UnityEngine;

public class AnimationEventRouter : MonoBehaviour
{
    private CreatureAnimatorController ctr;

    private void Awake()
    {
        // Ищем контроллер на родителе (или выше по иерархии)
        ctr = GetComponentInParent<CreatureAnimatorController>();
    }

    // Проброс событий
    public void SpawnProjectileEvent()
    {
        if (ctr != null) ctr.SpawnProjectileEvent();
    }

    public void HandleAttackHitEvent()
    {
        if (ctr != null) ctr.HandleAttackHitEvent();
    }

    public void RotateToAttackerEvent()
    {
        if (ctr != null) ctr.RotateToAttackerEvent();
    }
}
