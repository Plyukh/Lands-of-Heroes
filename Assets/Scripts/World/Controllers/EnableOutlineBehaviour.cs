using UnityEngine;

public class EnableOutlineBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Этот скрипт висит на стейте Enabled
        // Включаем active и выключаем inactive
        var cell = animator.GetComponentInParent<HexCell>();
        cell.InactiveOutline?.SetActive(false);
        cell.ActiveOutline?.SetActive(true);
    }
}
