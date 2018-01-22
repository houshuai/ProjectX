using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public class Archive
{
    public static Archive current;
    public bool isNew = true;
    public string title;
    public string currScene;

    public Inventory inventory = new Inventory();

    public static void Save(Archive[] archives)
    {
        var fileName = Application.persistentDataPath + "/saves.arc";
        var bf = new BinaryFormatter();
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
public class Inventory
{
    public string current;
    public List<string> itemList = new List<string>();
    
}
