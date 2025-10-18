using UnityEngine;

public class JoystickAnimatorController: MonoBehaviour
{
    [SerializeField] private Animator joystickAnimator;

    public void PlayJoystickClose()
    {
        joystickAnimator.SetTrigger("JoystickClose");
    }

    public void JoystickCloseEvent()
    {
        gameObject.SetActive(false);
    }
}
