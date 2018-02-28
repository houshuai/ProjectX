using UnityEngine;

public class MainMenu : Menu<MainMenu>
{
    private void Start()
    {
        Setting.Load();
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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
        Setting.Save();
        Application.Quit();
    }
}
