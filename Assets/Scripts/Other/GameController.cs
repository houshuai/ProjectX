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
    public Archive[] archives;

    private void OnEnable()
    {
        Scene.Instance.BeforeUnload += SaveCurrScene;
    }

    private void OnDisable()
    {
        Scene.Instance.BeforeUnload -= SaveCurrScene;
    }

    private void SaveCurrScene()
    {
        Archive.current.currScene = SceneManager.GetActiveScene().name;
        Archive.Save(archives);
    }

    public void LoadGame()
    {
        StartCoroutine(Scene.Instance.LoadScene(Archive.current.currScene));
    }

    public void Restart()
    {
        PauseMenu.Close();
        Scene.Instance.SwitchScene(Archive.current.currScene);
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
