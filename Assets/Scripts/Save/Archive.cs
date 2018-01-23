using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public class Archive
{
    public static Archive current;
    public bool isNew = true;
    public string title;
    public string currScene;
    public Dictionary<string, Vector3> positions = new Dictionary<string, Vector3>(3);
    public Inventory inventory = new Inventory();

    public void SetCurrentPosition(Vector3 position)
    {
        positions[currScene] = position;
    }

    public Vector3 GetCurrentPosition()
    {
        return positions[currScene];
    }

    public static void Save(Archive[] archives)
    {
        var fileName = Application.persistentDataPath + "/saves.arc";
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
        var fileName = Application.persistentDataPath + "/saves.arc";
        if (File.Exists(fileName))
        {
            var bf = new BinaryFormatter();
            var v3ss = new Vector3SerializationSurrogate();
            var ss = new SurrogateSelector();
            ss.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), v3ss);
            bf.SurrogateSelector = ss;
            using (var file = File.Open(fileName, FileMode.Open))
            {
                archives = (Archive[])bf.Deserialize(file);
            }
        }
        else
        {
            archives = new Archive[] { new Archive(), new Archive(), new Archive() };
        }

        return archives;
    }
}

[Serializable]
public sealed class Inventory
{
    public string current;
    public List<string> itemList = new List<string>();

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
