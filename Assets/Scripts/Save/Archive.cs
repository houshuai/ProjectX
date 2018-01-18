using System;
using System.Collections.Generic;

[Serializable]
public class Archive
{
    public static Archive current;
    public bool isNew = true;
    public string title;
    public string currScene;

    public Inventory inventory = new Inventory();
}

[Serializable]
public class Inventory
{
    public string current;
    public List<string> itemList = new List<string>();
    
}
