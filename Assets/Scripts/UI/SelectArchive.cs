using UnityEngine;
using UnityEngine.EventSystems;

public class SelectArchive : MonoBehaviour, ISelectHandler
{
    [HideInInspector]
    public Archive archive;

    public void OnSelect(BaseEventData eventData)
    {
        Archive.current = archive;
    }
}