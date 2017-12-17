using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private static GameController instance;
    public static GameController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameController>();
            }
            if (instance == null)
            {
                Debug.LogError("not found GameController");
            }
            return instance;
        }
    }

    public FloatVariable health;

    [HideInInspector]
    public Archive currArchive;

    private void OnEnable()
    {
        Scene.Instance.AfterLoaded += SaveCurrScene;
    }

    private void OnDisable()
    {
        Scene.Instance.AfterLoaded -= SaveCurrScene;
    }

    private void SaveCurrScene()
    {
        currArchive.currScene = SceneManager.GetActiveScene().name;
    }
    
    public void LoadGame(Archive archive)
    {
        currArchive = archive;

        StartCoroutine(Scene.Instance.LoadScene(currArchive.currScene));
    }

    public void Restart()
    {
        PauseMenu.Close();
        Scene.Instance.SwitchScene(currArchive.currScene);
        health.Value = 100;
    }

    public void ExitToMainMenu()
    {
        StartCoroutine(ExitToMainMenuAsync());
    }

    private IEnumerator ExitToMainMenuAsync()
    {
        PauseMenu.Close();
        yield return StartCoroutine(Scene.Instance.UnloadScene());
        MainMenu.Open();
    }
}
