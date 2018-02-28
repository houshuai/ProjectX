using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public OverlayGoods overlay;
    public Text toolTip;
    public Text content;

    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryMenu.Instance.GridItemClick(overlay);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        toolTip.gameObject.SetActive(true);
        toolTip.text = overlay.goods.description;
        toolTip.transform.position = eventData.position;
        content.text = toolTip.text;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        toolTip.gameObject.SetActive(false);
    }
}
