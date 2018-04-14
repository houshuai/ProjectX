using System;
using UnityEngine.UI;

public class PauseMenu : Menu<PauseMenu>
{
    public FloatVariable health;

    public Button resumeButton;
    public Button toScene1Button;
    public InputField seedInput;
    public Button setSeedButton;

    private TerrainController terrainController;

    private void OnEnable()
    {
        if (health.Value <= 0)
        {
            resumeButton.interactable = false;
        }
        if (Archive.current.currScene == "Scene3")
        {
            toScene1Button.gameObject.SetActive(true);
            seedInput.gameObject.SetActive(true);
            setSeedButton.gameObject.SetActive(true);
            terrainController = FindObjectOfType<TerrainController>();
            toScene1Button.onClick.AddListener(ToScene1);
            setSeedButton.onClick.AddListener(ResetSeed);
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

    private void ToScene1()
    {
        GameController.Instance.ChangeScene("Scene1");
    }

    private void ResetSeed()
    {
        terrainController.ReInitialTerrain(Convert.ToInt32(seedInput.text));
    }
}
