using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SelectArchive : MonoBehaviour, IPointerDownHandler
{
    [HideInInspector]
    public Archive archive;

    private Image background;

    private void Awake()
    {
        background = GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Archive.current = archive;
        ArchiveMenu.Instance.OnSelect();
        background.color = Color.red;
    }
}