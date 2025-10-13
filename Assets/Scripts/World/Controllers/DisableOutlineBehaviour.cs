using UnityEngine;

/// <summary>
/// StateMachineBehaviour ��� ��������� �Disabled� � ���������.
/// ��� ����� � ����� ��������� ��� �������, �������� ������ Inactive.
/// </summary>
public class DisableOutlineBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var cell = animator.GetComponentInParent<HexCell>();
        if (cell != null)
        {
            cell.SetOutlineState(OutlineState.Inactive);
        }
    }
}