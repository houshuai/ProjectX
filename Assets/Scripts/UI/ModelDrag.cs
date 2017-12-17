using UnityEngine;
using UnityEngine.EventSystems;

public class ModelDrag : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [HideInInspector]
    public Transform model;

    private bool isDrag;
    private float preX;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDrag = true;
        preX = Input.mousePosition.x;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDrag = false;
    }

    private void Update()
    {
        if (isDrag && model != null)
        {
            var rotation = Quaternion.Euler(0, preX - Input.mousePosition.x, 0);
            model.rotation = model.rotation * rotation;
            preX = Input.mousePosition.x;
        }
    }
}
