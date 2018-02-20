using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene : MonoBehaviour
{
    private static Scene instance;
    public static Scene Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Scene>();
            }
            if (instance == null)
            {
                Debug.Log("not found SceneManager");
            }
            return instance;
        }
    }

    public event Action BeforeUnload;
    public event Action AfterLoaded;
    public GameObject projectXSceneCamera;
    public GameObject LoadingText;
    

    public void SwitchScene(string sceneName)
    {
        StartCoroutine(SwitchSceneAsync(sceneName));
    }

    private IEnumerator SwitchSceneAsync(string name)
    {
        yield return StartCoroutine(UnloadScene());

        yield return StartCoroutine(LoadScene(name));
    }

    public IEnumerator UnloadScene()
    {
        if (BeforeUnload != null)
        {
            BeforeUnload();
        }
        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        projectXSceneCamera.SetActive(true);
    }

    public IEnumerator LoadScene(string name)
    {
        LoadingText.SetActive(true);

        yield return SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
        var newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        SceneManager.SetActiveScene(newScene);

        LoadingText.SetActive(false);
        projectXSceneCamera.SetActive(false);

        if (AfterLoaded != null)
        {
            AfterLoaded();
        }
    }
}
