using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private static Dictionary<string, ButtonState> buttons = new Dictionary<string, ButtonState>();

    public string buttonName;
    public Sprite upSprite;
    public Sprite overSprite;

    private ButtonState button = new ButtonState();
    private Image image;

    private void Start()
    {
        image = GetComponent<Image>();
        buttons.Add(buttonName, button);
    }

    private void OnDestroy()
    {
        buttons.Remove(buttonName);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button.pressed)
        {
            return;
        }
        button.pressed = true;
        button.lastPressedFrame = Time.frameCount;
        if (overSprite!=null)
        {
            image.sprite = overSprite;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        button.pressed = false;
        button.releasedFrame = Time.frameCount;
        if (upSprite!=null)
        {
            image.sprite = upSprite;
        }
    }

    public static bool GetButton(string name)
    {
        return buttons[name].pressed;
    }

    public static bool GetButtonDown(string name)
    {
        return buttons[name].lastPressedFrame == Time.frameCount - 1;
    }

    public static bool GetButtonUp(string name)
    {
        return buttons[name].releasedFrame == Time.frameCount - 1;
    }
}

public class ButtonState
{
    public int lastPressedFrame = -5;
    public int releasedFrame = -5;
    public bool pressed;
}
