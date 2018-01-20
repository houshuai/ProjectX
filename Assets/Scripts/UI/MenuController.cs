using System.Collections.Generic;
using UnityEngine;

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
    }

    private void OnDisable()
    {
        health.ValueChanged -= HealthChanged;
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
                //Cursor.lockState = CursorLockMode.Confined;
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
            //Cursor.lockState = CursorLockMode.Confined;
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
            //Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void CloseAll()
    {
        while (menuStack.Count > 0)
        {
            Destroy(menuStack.Pop().gameObject);
        }
        //Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
