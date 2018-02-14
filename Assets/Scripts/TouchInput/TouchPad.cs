using UnityEngine;
using UnityEngine.EventSystems;

public class TouchPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private static float x, y;
    private static string xName = "X";
    private static string yName = "Y";

    public float xSensitivity = 1f;
    public float ySensitivity = 1f;

    private bool isDraging;
    private int id;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDraging = true;
        id = eventData.pointerId;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDraging = false;
        id = -1;
        x = 0;
        y = 0;
    }

    private void Update()
    {
        if (!isDraging)
        {
            return;
        }

        if (Input.touchCount > id && id != -1)
        {
            var delta = Input.touches[id].deltaPosition.normalized;
            x = delta.x;
            y = delta.y;
        }
    }

    public static float GetAxis(string name)
    {
        if (name == xName)
        {
            return x;
        }
        else if (name == yName)
        {
            return y;
        }
        else
        {
            return 0;
        }
    }
}
