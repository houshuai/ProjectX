using UnityEngine;

/// <summary>
/// 战利品
/// </summary>
public class Loot : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tags.Player))
        {
            var goods = GoodsDictionary.GetRandom(GoodsType.common);
            Archive.current.inventories[(int)GoodsType.common].In(goods);
            gameObject.SetActive(false);
        }
    }
}
