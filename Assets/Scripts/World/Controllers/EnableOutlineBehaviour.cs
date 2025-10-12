using UnityEngine;

/// <summary>
/// StateMachineBehaviour ��� ��������� �Enabled� � ���������.
/// ��� ����� � ����� �������� Active-�������.
/// </summary>
public class EnableOutlineBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var cell = animator.GetComponentInParent<HexCell>();
        if (cell != null)
        {
            cell.SetOutlineState(OutlineState.Active);
        }
    }
}
