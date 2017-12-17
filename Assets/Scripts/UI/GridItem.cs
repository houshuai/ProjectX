using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridItem : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        var image = gameObject.GetComponent<Image>();
        if (image != null)
        {
            InventoryMenu.Instance.SelectItem(image.sprite.name);
        }
    }
    
}
