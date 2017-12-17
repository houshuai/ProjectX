using UnityEngine;
using UnityEngine.UI;

public class NewArchiveMenu : Menu<NewArchiveMenu>
{
    [HideInInspector]
    public static Archive archive;

    public static void Open(Archive archive)
    {
        NewArchiveMenu.archive = archive;
        Open();
    }

    public void OnConfirm()
    {
        archive.title = GetComponentInChildren<InputField>().text;
        archive.currScene = "Scene1";
        archive.isNew = false;

        MenuController.Instance.CloseAll();
        GameController.Instance.LoadGame(archive);
    }
}
