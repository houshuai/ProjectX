using System.IO;
using UnityEngine;
using XLua;

public class LuaLoader : MonoBehaviour
{
    public static LuaEnv luaEnv = new LuaEnv();
    public static float lastGCTime = 0;
    public const float GCInterval = 1;

    private static bool isLoaded = false;

    private static void CopyToPersistent()
    {
        var bundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/Lua/luafile");
        var allTxt = bundle.LoadAllAssets<TextAsset>();
        foreach (var txt in allTxt)
        {
            using (var sw = File.CreateText(Application.persistentDataPath + "/" + txt.name))
            {
                sw.Write(txt.text);
            }
        }
    }

    /// <summary>
    /// 自定义加载，xlua的文档建议全局使用一个DoString加载main.lua，然后在main.lua里加载别的lua文件，
    /// 但是我不会在main.lua里加载的其他lua之前进行注入到其他lua里，导致其他lua调用start时由于没有注入cs里的对象会出现nil，
    /// 按理说在调用start之前已经注入了呀
    /// </summary>
    private static void Load()
    {
        luaEnv.AddLoader((ref string fileName) =>
        {
            var filePath = Application.persistentDataPath + "/" + fileName;
            string str = "";
            if (File.Exists(filePath))
            {
                using (var sr = new StreamReader(filePath))
                {
                    str = sr.ReadToEnd();
                }
            }
            return System.Text.Encoding.UTF8.GetBytes(str);
        });
        luaEnv.DoString("require 'main.lua'");  //带.lua后缀，customloader里需要用来读文件，main.lua里的require同理
    }

    private static string Read(string fileName)
    {
#if UNITY_EDITOR
        var filePath = Application.dataPath + "/Scripts/Lua/" + fileName + ".txt";
#else
        var filePath = Application.persistentDataPath + "/" + fileName;
#endif
        string str = "";
        if (File.Exists(filePath))
        {
            using (var sr = new StreamReader(filePath))
            {
                str = sr.ReadToEnd();
            }
        }
        return str;
    }

    /// <summary>
    /// 获取lua表，从文件中读取lua文件
    /// </summary>
    /// <param name="injections">需要注入的对象</param>
    /// <param name="self">LuaBehaviour本身</param>
    /// <param name="fileName">文件名，需要.lua后缀</param>
    /// <returns></returns>
    public static LuaTable GetLuaTable(Injection[] injections, LuaBehaviour self, string fileName)
    {
#if !UNITY_EDITOR
        //第一次打开并获取lua时将ab包解压到persistent文件夹里
        if (!isLoaded)
        {
            CopyToPersistent();
            isLoaded = true;
        }
#endif

        var result = luaEnv.NewTable();
        var meta = luaEnv.NewTable();
        meta.Set("__index", luaEnv.Global);
        result.SetMetaTable(meta);
        meta.Dispose();

        //注入
        result.Set("self", self);
        foreach (var injection in injections)
        {
            result.Set(injection.name, injection.gameObject);
        }
        //注入之后再加载
        luaEnv.DoString(Read(fileName), fileName, result);
        
        return result;
    }

    private void Update()
    {
        if (Time.time - lastGCTime > GCInterval)
        {
            luaEnv.Tick();
            lastGCTime = Time.time;
        }
    }
}
