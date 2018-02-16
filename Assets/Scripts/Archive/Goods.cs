using System;
using System.IO;
using UnityEngine;

[Serializable]
public class Goods
{
    public int id;
    public string name;
}

[Serializable]
public class OverlayGoods
{
    public Goods goods;
    public int count;

    public OverlayGoods(Goods goods)
    {
        this.goods = goods;
        count = 1;
    }
}

public static class GoodsDictionary
{
    private static Goods[] goodsCategory = new Goods[]
    {
        new Goods() {id=0, name="bathtowel"},
        new Goods() {id=1, name="blazer"},
        new Goods() {id=2, name="casualwear"},
        new Goods() {id=3, name="casualwear1"},
        new Goods() {id=4, name="jersey"},
        new Goods() {id=5, name="schoolwear"},
        new Goods() {id=6, name="schoolwear1"},
        new Goods() {id=7, name="schoolwear2"},
        new Goods() {id=8, name="swimwear"}
    };

    public static int Count { get { return goodsCategory.Length; } }

    static GoodsDictionary()
    {
        //Initial();
        //var fileName = Application.streamingAssetsPath + "/GoodsCategory.txt";
        //if (File.Exists(fileName))
        //{
        //    using (var sr = new StreamReader(fileName))
        //    {
        //        goodsCategory = JsonHelper.ArrayFromJson<Goods>(sr.ReadToEnd());
        //    }
        //}
    }

    public static Goods Get(int id)
    {
        foreach (var goods in goodsCategory)
        {
            if (goods.id == id)
            {
                return goods;
            }
        }

        return null;
    }

    private static void Initial()
    {
        var array = new Goods[]
        {
            new Goods(){id=1,name="bathtowel"},
            new Goods(){id=2,name="blazer"},
            new Goods(){id=3,name="casualwear"},
            new Goods(){id=4,name="casualwear1"},
            new Goods(){id=5,name="jersey"},
            new Goods(){id=6,name="schoolwear"},
            new Goods(){id=7,name="schoolwear1"},
            new Goods(){id=8,name="schoolwear2"},
            new Goods(){id=9,name="swimwear"}
        };

        string json = JsonHelper.ArrayToJson(array);

        var fileName = Application.streamingAssetsPath + "/GoodsCategory.txt";
        using (var sw = new StreamWriter(fileName))
        {
            sw.WriteLine(json);
        }
    }
}

public class JsonHelper
{
    public static T[] ArrayFromJson<T>(string json)
    {
        var wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.array;
    }

    public static string ArrayToJson<T>(T[] array)
    {
        var wrapper = new Wrapper<T>() { array = array };
        return JsonUtility.ToJson(wrapper);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}