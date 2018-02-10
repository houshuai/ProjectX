using UnityEngine;
using UnityEngine.EventSystems;

public class GridItem : MonoBehaviour, IPointerClickHandler
{
    public OverlayGoods overlay;

    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryMenu.Instance.GridItemClick(overlay);
    }

}
