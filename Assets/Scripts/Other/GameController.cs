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
    [HideInInspector]
    public Transform player;

    private bool isRestart;

    private void OnEnable()
    {
        Scene.Instance.BeforeUnload += SceneBeforeUnload;
        Scene.Instance.AfterLoaded += SceneAfterLoaded;
    }

    private void OnDisable()
    {
        Scene.Instance.BeforeUnload -= SceneBeforeUnload;
        Scene.Instance.AfterLoaded -= SceneAfterLoaded;
    }

    private void SceneBeforeUnload()
    {
        if (!isRestart)
        {
            Archive.current.SetPlayerPosition(player.position+new Vector3(0,0,1));
            Archive.Save(archives);
        }
    }

    private void SceneAfterLoaded()
    {
        Archive.current.currScene = SceneManager.GetActiveScene().name;
        Vector3 pos;
        if (Archive.current.GetPlayerPosition(out pos))
        {
            player.position = pos + new Vector3(0, 1, 0);
        }
    }

    public void LoadGame()
    {
        StartCoroutine(Scene.Instance.LoadScene(Archive.current.currScene));
    }

    public void Restart()
    {
        isRestart = true;
        PauseMenu.Close();
        Scene.Instance.SwitchScene(Archive.current.currScene);
        health.Value = 100;
        isRestart = false;
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
