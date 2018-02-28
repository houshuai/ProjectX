using System;

public enum GoodsType
{
    common = 0,
    cloth = 1,
    count = 2,
}

/// <summary>
/// 物品的基本信息
/// </summary>
[Serializable]
public class Goods
{
    public int id;
    public string name;
    public string description;
    public GoodsType type;
    public int price;

    public static bool operator ==(Goods left, Goods right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (ReferenceEquals(left, null))
        {
            return false;
        }

        if (ReferenceEquals(right, null))
        {
            return false;
        }

        return left.id == right.id;
    }

    public static bool operator !=(Goods left, Goods right)
    {
        return !(left == right);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Goods);
    }

    public bool Equals(Goods other)
    {
        if (ReferenceEquals(other,null))
        {
            return false;
        }

        if (ReferenceEquals(this,other))
        {
            return true;
        }

        return id == other.id;
    }

    public override int GetHashCode()
    {
        return 0;
    }
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