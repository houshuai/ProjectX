using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class OpenMenu : MonoBehaviour
{
    public string menu;

    void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(Open);
    }

    private void Open()
    {
        if (menu=="PauseMenu")
        {
            MenuController.Instance.OpenMenu<PauseMenu>();
        }
        else if (menu=="InventoryMenu")
        {
            MenuController.Instance.OpenMenu<InventoryMenu>();
        }
    }
}
