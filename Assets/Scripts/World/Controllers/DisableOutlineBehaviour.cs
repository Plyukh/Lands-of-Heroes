using UnityEngine;

public class DisableOutlineBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // ���� ������ ����� �� ������ Disabled
        // ������ ��������� ��������; actual Off � � OnStateExit
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // �� ������ �� Disabled ������ ��������� �������� ������
        var cell = animator.GetComponentInParent<HexCell>();
        cell.ActiveOutline?.SetActive(false);
        cell.InactiveOutline?.SetActive(false);
    }
}
