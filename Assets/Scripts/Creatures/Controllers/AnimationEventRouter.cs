using UnityEngine;

public class AnimationEventRouter : MonoBehaviour
{
    private CreatureAnimatorController ctr;

    private void Awake()
    {
        // ���� ���������� �� �������� (��� ���� �� ��������)
        ctr = GetComponentInParent<CreatureAnimatorController>();
    }

    // ������� �������
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
