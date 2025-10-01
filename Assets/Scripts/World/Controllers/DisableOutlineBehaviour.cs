using UnityEngine;

public class DisableOutlineBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Этот скрипт висит на стейте Disabled
        // Только запускаем анимацию; actual Off — в OnStateExit
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // По выходу из Disabled стейта полностью скрываем контур
        var cell = animator.GetComponentInParent<HexCell>();
        cell.ActiveOutline?.SetActive(false);
        cell.InactiveOutline?.SetActive(false);
    }
}
