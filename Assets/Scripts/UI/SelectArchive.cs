using UnityEngine;
using UnityEngine.EventSystems;

public class SelectArchive : MonoBehaviour, IPointerDownHandler
{
    [HideInInspector]
    public Archive archive;

    public void OnPointerDown(PointerEventData eventData)
    {
        ArchiveMenu.Instance.OnArchive(archive);
    }
}