using UnityEngine;

public class JoystickAnimatorController: MonoBehaviour
{
    [SerializeField] private Animator joystickAnimator;

    public void SetAction(int action, bool value)
    {
        joystickAnimator.SetBool(action + ".Action", value);
    }

    public void PlayJoystickClose()
    {
        joystickAnimator.SetTrigger("JoystickClose");
    }

    public void JoystickCloseEvent()
    {
        gameObject.SetActive(false);
    }
}
