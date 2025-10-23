using UnityEngine;

public class JoystickAnimatorController: MonoBehaviour
{
    [SerializeField] private Animator joystickAnimator;

    public void SelectJoystick(int actionCount)
    {
        joystickAnimator.SetTrigger(actionCount + ".ActionOpen");
    }
    
    public void SetAction(int action, bool value)
    {
        joystickAnimator.SetBool(action + ".Action", value);
    }

    public void PlayJoystickClose(int actionCount)
    {
        joystickAnimator.SetTrigger(actionCount + ".ActionClose");
    }

    public void JoystickCloseEvent()
    {
        gameObject.SetActive(false);
    }
}
