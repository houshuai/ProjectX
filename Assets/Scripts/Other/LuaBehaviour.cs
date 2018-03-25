using System;
using UnityEngine;
using XLua;

public class LuaBehaviour : MonoBehaviour
{
    [Tooltip("解压到persistent里的名字")]
    public string fileName;
    public Injection[] injections;

    private LuaTable env;
    private Action luaStart, luaUpdate, luaDestroy;

    private void Awake()
    {
        env = LuaLoader.GetLuaTable(injections, this, fileName);
        
        var luaAwake = env.Get<Action>("awake");
        luaStart = env.Get<Action>("start");
        luaUpdate = env.Get<Action>("update");
        luaDestroy = env.Get<Action>("destroy");

        luaAwake?.Invoke();
    }

    private void Start()
    {
        luaStart?.Invoke();
    }

    private void Update()
    {
        luaUpdate?.Invoke();
    }

    private void OnDestroy()
    {
        luaDestroy?.Invoke();
        luaStart = null;
        luaUpdate = null;
        luaDestroy = null;
        env.Dispose();
    }
}

[Serializable]
public class Injection
{
    public string name;
    public GameObject gameObject;
}
