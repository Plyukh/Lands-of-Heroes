using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct ActionIconData
{
    public JoystickActionType actionType;
    public Sprite iconSprite;
    public Sprite backgroundSprite;
    public Sprite borderSprite;

    public Color enabledColor;
    public Color disabledColor;

    public Rect rect;
}
