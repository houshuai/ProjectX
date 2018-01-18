using UnityEngine;
using UnityEngine.EventSystems;

public class SelectArchive : MonoBehaviour, IPointerDownHandler
{
    [HideInInspector]
    public Archive archive;

    public void OnPointerDown(PointerEventData eventData)
    {
        Archive.current = archive;
        ArchiveMenu.Instance.OnArchive();
    }
}