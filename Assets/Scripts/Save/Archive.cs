using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Archive : ScriptableObject
{
    public bool isNew = true;
    public string title;
    public string currScene;

    public Inventory inventory;
}

[Serializable]
public class Inventory
{
    public string current;
    public List<string> itemList;

    public Inventory()
    {
        itemList = new List<string>();
    }
}
