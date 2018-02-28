using System;
using System.Collections.Generic;

[Serializable]
public sealed class Inventory
{
    public int capacity;
    public int overlay;
    public List<OverlayGoods> itemList;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="capacity">容量</param>
    /// <param name="overlay">最大叠加数</param>
    public Inventory(int capacity, int overlay)
    {
        this.capacity = capacity;
        this.overlay = overlay;
        itemList = new List<OverlayGoods>(capacity);
    }

    /// <summary>
    /// 添加物品
    /// </summary>
    /// <param name="newGoods"></param>
    /// <returns></returns>
    public bool In(Goods newGoods)
    {
        foreach (var item in itemList)
        {
            if (item.goods.id == newGoods.id && item.count < overlay)
            {
                item.count++;
                return true;
            }
        }

        if (itemList.Count < capacity)
        {
            itemList.Add(new OverlayGoods(newGoods));
            return true;
        }

        return false;
    }

    /// <summary>
    /// 取出物品
    /// </summary>
    /// <param name="id">物品id</param>
    /// <returns></returns>
    public Goods Out(int id)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].goods.id == id)
            {
                var item = itemList[i];
                item.count--;
                if (item.count == 0)
                {
                    itemList.RemoveAt(i);
                }
                return item.goods;
            }
        }

        return null;
    }
}
