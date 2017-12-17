using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : Menu<PauseMenu>
{
    public FloatVariable health;

    private Button resumeButton;

    private void OnEnable()
    {
        resumeButton = GetComponentInChildren<Button>();
        if (health.Value <= 0)
        {
            resumeButton.interactable = false;
        }
    }

    private void Destroy()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Restart()
    {
        GameController.Instance.Restart();
    }

    public void OnOption()
    {
        OptionMenu.Open();
    }

    public void ExitToMainMenu()
    {
        GameController.Instance.ExitToMainMenu();
    }

}
