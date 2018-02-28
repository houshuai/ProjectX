using UnityEngine;
using UnityEngine.EventSystems;

public class ModelDrag : MonoBehaviour, IDragHandler
{
    [HideInInspector]
    public Transform model;

    public void OnDrag(PointerEventData eventData)
    {
        var rotation = Quaternion.Euler(0, -eventData.delta.x, 0);
        model.rotation = model.rotation * rotation;
    }
}
