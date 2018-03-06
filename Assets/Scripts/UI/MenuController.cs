using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance { get; private set; }
    public MainMenu mainMenu;
    public ArchiveMenu archiveMenu;
    public NewArchiveMenu newArchiveMenu;
    public OptionMenu optionMenu;
    public AudioMenu audioMenu;
    public VisualMenu visualMenu;
    public PauseMenu pauseMenu;
    public InventoryMenu inventoryMenu;
    public FloatVariable health;
    public GameObject HUB;
    public GameObject charactorButton;
    public GameObject jumpButton;
    public GameObject fireButton;

    private Stack<Menu> menuStack;

    private void Awake()
    {
        Instance = this;
        menuStack = new Stack<Menu>();
        health.Value = 100;

        MainMenu.Open();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void OnEnable()
    {
        health.ValueChanged += HealthChanged;
        Scene.Instance.BeforeUnload += BeforeUnload;
        Scene.Instance.AfterLoaded += AfterLoaded;
    }

    private void OnDisable()
    {
        health.ValueChanged -= HealthChanged;
        if (Scene.Instance != null)
        {
            Scene.Instance.BeforeUnload -= BeforeUnload;
            Scene.Instance.AfterLoaded -= AfterLoaded;
        }
    }

    private void BeforeUnload()
    {
        if (SceneManager.GetActiveScene().name == "Scene2")
        {
            charactorButton.SetActive(true);
        }
        jumpButton.SetActive(true);    //防止切换到其他场景GameObject.Find()无法找到
        fireButton.SetActive(true);
        HUB.SetActive(false);
    }

    private void AfterLoaded()
    {
        HUB.SetActive(true);
        if (SceneManager.GetActiveScene().name == "Scene2")
        {
            charactorButton.SetActive(false);
        }

    }

    private void HealthChanged(object sender, ValueChangedEventArgs e)
    {
        if (health.Value <= 0)
        {
            PauseMenu.Open();
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (menuStack.Count == 0)
            {
                PauseMenu.Open();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (!(menuStack.Peek() as MainMenu))
            {
                CloseMenu();
            }
        }

        if (Input.GetKeyDown(KeyCode.I) && menuStack.Count == 0)
        {
            InventoryMenu.Open();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void OpenMenu<T>() where T : Menu
    {
        if (menuStack.Count > 0)
        {
            menuStack.Peek().gameObject.SetActive(false);
        }

        var prefab = GetPrefab<T>();
        var newMenu = Instantiate(prefab, transform);
        menuStack.Push(newMenu);
    }

    private T GetPrefab<T>() where T : Menu
    {
        if (typeof(T) == typeof(MainMenu))
        {
            return mainMenu as T;
        }
        else if (typeof(T) == typeof(ArchiveMenu))
        {
            return archiveMenu as T;
        }
        else if (typeof(T) == typeof(NewArchiveMenu))
        {
            return newArchiveMenu as T;
        }
        else if (typeof(T) == typeof(OptionMenu))
        {
            return optionMenu as T;
        }
        else if (typeof(T) == typeof(AudioMenu))
        {
            return audioMenu as T;
        }
        else if (typeof(T) == typeof(VisualMenu))
        {
            return visualMenu as T;
        }
        else if (typeof(T) == typeof(PauseMenu))
        {
            return pauseMenu as T;
        }
        else if (typeof(T) == typeof(InventoryMenu))
        {
            return inventoryMenu as T;
        }

        throw new KeyNotFoundException("the menu type is not found");
    }

    public void CloseMenu()
    {
        if (menuStack.Count > 0)
        {
            var menu = menuStack.Pop();
            Destroy(menu.gameObject);
        }
        if (menuStack.Count > 0)
        {
            menuStack.Peek().gameObject.SetActive(true);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void CloseAll()
    {
        while (menuStack.Count > 0)
        {
            Destroy(menuStack.Pop().gameObject);
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
