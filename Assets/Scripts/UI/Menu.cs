using UnityEngine;

public abstract class Menu<T> : Menu where T : Menu<T>
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        Instance = (T)this;
    }

    protected virtual void OnDestroy()
    {
        Instance = null;
    }

    public static void Open()
    {
        if (Instance!=null)
        {
            return;
        }
        MenuController.Instance.OpenMenu<T>();
    }

    public static void Close()
    {
        if (Instance==null)
        {
            return;
        }
        MenuController.Instance.CloseMenu();
    }

    public override void Back()
    {
        Close();
    }
}

public abstract class Menu : MonoBehaviour
{
    public abstract void Back();
}
