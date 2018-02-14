using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public float moveRange = 100;
    public Sprite upSprite;
    public Sprite overSprite;

    private Vector3 originalPos;
    private Image image;

    private static float h, v;
    private static string horizontalName = "Horizontal";
    private static string verticalName = "Vertical";

    private void Start()
    {
        originalPos = transform.position;
        image = GetComponent<Image>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        h = eventData.position.x - originalPos.x;
        h = Mathf.Clamp(h, -moveRange, moveRange);
        v = eventData.position.y - originalPos.y;
        v = Mathf.Clamp(v, -moveRange, moveRange);

        transform.position = new Vector3(originalPos.x + h, originalPos.y + v, originalPos.z);

        h /= moveRange;
        v /= moveRange;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        image.sprite = overSprite;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.position = originalPos;
        image.sprite = upSprite;
        h = 0;
        v = 0;
    }

    public static float GetAxis(string name)
    {
        if (name == horizontalName)
        {
            return h;
        }
        else if (name == verticalName)
        {
            return v;
        }
        else
        {
            return 0;
        }
    }
}
