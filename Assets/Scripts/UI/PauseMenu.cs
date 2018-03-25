using UnityEngine.UI;

public class PauseMenu : Menu<PauseMenu>
{
    public FloatVariable health;

    public Button resumeButton;
    public Button toScene1Button;

    private void OnEnable()
    {
        if (health.Value <= 0)
        {
            resumeButton.interactable = false;
        }
        if (Archive.current.currScene == "Scene3")
        {
            toScene1Button.gameObject.SetActive(true);
        }
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

    public void ToScene1()
    {
        GameController.Instance.ChangeScene("Scene1");
    }
}
