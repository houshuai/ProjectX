using UnityEngine;

public class MainMenu : Menu<MainMenu>
{
    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnStart()
    {
        ArchiveMenu.Open();
    }

    public void OnOption()
    {
        OptionMenu.Open();
    }

    public override void Back()
    {
        Application.Quit();
    }
}
