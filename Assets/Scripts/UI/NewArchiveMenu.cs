using UnityEngine.UI;

public class NewArchiveMenu : Menu<NewArchiveMenu>
{
    public void OnConfirm()
    {
        var archive = Archive.current;
        archive.title = GetComponentInChildren<InputField>().text;
        archive.currScene = "Scene1";
        archive.isNew = false;

        MenuController.Instance.CloseAll();
        GameController.Instance.LoadGame();
    }
}
