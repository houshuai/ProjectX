using System;
using System.Collections.Generic;

[Serializable]
public sealed class Inventory
{
    public int capacity;
    public int overlay;
    public List<OverlayGoods> itemList;

    public Inventory(int capacity, int overlay)
    {
        this.capacity = capacity;
        this.overlay = overlay;
        itemList = new List<OverlayGoods>(capacity);
    }

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

    public Goods Out(Goods goods)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].goods.id == goods.id)
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
