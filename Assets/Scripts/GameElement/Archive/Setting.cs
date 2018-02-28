using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

/// <summary>
/// 存储游戏设置
/// </summary>
[Serializable]
public class Setting
{
    public static Setting instance;

    public int resolution = 0;
    public int quality = 0;
    public bool reflection = true;
    public bool refraction = true;
    public float masterVolume = 0;
    public float sfxVolume = 0;
    public float musicVolume = 0;
    
    /// <summary>
    /// 保存设置到文件
    /// </summary>
    public static void Save()
    {
        var fileName = Application.persistentDataPath + "/setting.save";
        var bf = new BinaryFormatter();
        using (var file = File.Create(fileName))
        {
            bf.Serialize(file, instance);
        }
    }

    /// <summary>
    /// 从文件中读取设置
    /// </summary>
    public static void Load()
    {
        var fileName = Application.persistentDataPath + "/setting.save";
        var bf = new BinaryFormatter();
        if (File.Exists(fileName))
        {
            using (var file = File.OpenRead(fileName))
            {
                instance = (Setting)bf.Deserialize(file);
            }
        }
        else
        {
            instance = new Setting();
        }
    }
}
