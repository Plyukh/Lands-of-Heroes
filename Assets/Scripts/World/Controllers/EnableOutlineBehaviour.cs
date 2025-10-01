using UnityEngine;

public class EnableOutlineBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // ���� ������ ����� �� ������ Enabled
        // �������� active � ��������� inactive
        var cell = animator.GetComponentInParent<HexCell>();
        cell.InactiveOutline?.SetActive(false);
        cell.ActiveOutline?.SetActive(true);
    }
}
