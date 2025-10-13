using UnityEngine;

/// <summary>
/// StateMachineBehaviour для состояния «Disabled» в аниматоре.
/// При входе в стейт отключает все обводки, оставляя только Inactive.
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