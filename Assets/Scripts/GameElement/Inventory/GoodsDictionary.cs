using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 保存所有物品的信息
/// </summary>
public class GoodsDictionary : MonoBehaviour
{
    private static List<Goods>[] allGoods;

    private IEnumerator Start()
    {
        //Initial();
        var fileName = Application.streamingAssetsPath + "/GoodsCategory.txt";
        if (File.Exists(fileName))
        {
            using (var www = new WWW(fileName))
            {
                yield return www;
                var array = JsonHelper.ArrayFromJson<Goods>(www.text);

                allGoods = new List<Goods>[(int)GoodsType.count]
                {
                    new List<Goods>(),
                    new List<Goods>(),
                };
                foreach (var goods in array)
                {
                    allGoods[(int)goods.type].Add(goods);
                }
            }
            
        }
        else
        {
            Debug.LogWarning("not found GoodsCategory.txt");
        }

    }

    /// <summary>
    /// 根据id获得物品
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Goods Get(int id)
    {
        foreach (var all in allGoods)
        {
            foreach (var goods in all)
            {
                if (goods.id == id)
                {
                    return goods;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 获得所有指定类型的物品
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<Goods> Get(GoodsType type)
    {
        return allGoods[(int)type];
    }
    
    /// <summary>
    /// 随机获得一个指定类型的物品
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static Goods GetRandom(GoodsType type)
    {
        var all = allGoods[(int)type];
        var random = UnityEngine.Random.Range(0, all.Count - 1);
        return all[random];
    }

    private void Initial()
    {
        var array = new Goods[]
        {
            new Goods() {id=0, name="bathtowel", description="浴巾", type=GoodsType.cloth, price=2},
            new Goods() {id=1, name="blazer", description="西装", type=GoodsType.cloth, price=3},
            new Goods() {id=3, name="casualwear1", description="便服1", type=GoodsType.cloth, price=2},
            new Goods() {id=4, name="jersey", description="针织衫", type=GoodsType.cloth, price=3},
            new Goods() {id=5, name="schoolwear", description="校服", type=GoodsType.cloth, price=2},
            new Goods() {id=6, name="schoolwear1", description="校服1", type=GoodsType.cloth, price=2},
            new Goods() {id=7, name="schoolwear2", description="校服2", type=GoodsType.cloth, price=2},
            new Goods() {id=8, name="swimwear", description="泳衣", type=GoodsType.cloth, price=3},
            new Goods() {id=9, name="axe", description="斧子", type=GoodsType.common, price=1 },
            new Goods() {id=10, name="bag", description="包", type=GoodsType.common, price=1},
            new Goods() {id=11, name="bottle", description="瓶", type=GoodsType.common, price=1},
            new Goods() {id=12, name="bowl", description="锅", type=GoodsType.common, price=1},
            new Goods() {id=13, name="chest", description="宝藏", type=GoodsType.common, price=1},
            new Goods() {id=14, name="cloth", description="衣服", type=GoodsType.common, price=1}
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
