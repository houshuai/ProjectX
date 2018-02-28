using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public class Archive
{
    /// <summary>
    /// 游戏当前运行的存档
    /// </summary>
    public static Archive current;

    public bool isNew = true;
    public string title;                             //存档的名称
    public string currScene;                         //角色所在的场景
    public Dictionary<string, Vector3> positions;    //角色在每个场景最后的位置
    public Goods currGoods;                          //角色当前装备了的物品
    public Inventory[] inventories;                  //角色的背包物品
    public int gemCount;                             //角色的宝石数量

    public void Initial()
    {
        isNew = true;
        title = null;
        currScene = null;
        positions = new Dictionary<string, Vector3>(3);
        currGoods = null;
        inventories = new Inventory[(int)GoodsType.count]
        {
            new Inventory(12,3),
            new Inventory(12,1),
        };
    }

    /// <summary>
    /// 保存当前场景玩家的位置
    /// </summary>
    /// <param name="position"></param>
    public void SetPlayerPosition(Vector3 position)
    {
        if (positions.ContainsKey(currScene))
        {
            positions[currScene] = position;
        }
        else
        {
            positions.Add(currScene, position);
        }
    }

    /// <summary>
    /// 获取当前场景角色的位置
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool GetPlayerPosition(out Vector3 position)
    {
        if (positions.ContainsKey(currScene))
        {
            position = positions[currScene];
            return true;
        }
        else
        {
            position = new Vector3();
            return false;
        }
    }

    public static void Save(Archive[] archives)
    {
        var fileName = Application.persistentDataPath + "/archives.save";
        var bf = new BinaryFormatter();
        var v3ss = new Vector3SerializationSurrogate();
        var ss = new SurrogateSelector();
        ss.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), v3ss);
        bf.SurrogateSelector = ss;
        using (var file = File.Create(fileName))
        {
            bf.Serialize(file, archives);
        }
    }

    public static Archive[] Load()
    {
        Archive[] archives = null;
        var fileName = Application.persistentDataPath + "/archives.save";
        if (File.Exists(fileName))
        {
            var bf = new BinaryFormatter();
            var v3ss = new Vector3SerializationSurrogate();
            var ss = new SurrogateSelector();
            ss.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), v3ss);
            bf.SurrogateSelector = ss;
            using (var file = File.OpenRead(fileName))
            {
                archives = (Archive[])bf.Deserialize(file);
            }
        }
        else
        {
            archives = new Archive[] { new Archive(), new Archive(), new Archive() };
            foreach (var archive in archives)
            {
                archive.Initial();
            }
        }

        return archives;
    }
}

public sealed class Vector3SerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        var v = (Vector3)obj;
        info.AddValue("x", v.x);
        info.AddValue("y", v.y);
        info.AddValue("z", v.z);

    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        var v = (Vector3)obj;
        v.x = info.GetSingle("x");
        v.y = info.GetSingle("y");
        v.z = info.GetSingle("z");
        return v;
    }
}
