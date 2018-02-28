using UnityEngine;

/// <summary>
/// 战利品
/// </summary>
public class Loot : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == Tags.Player)
        {
            Archive.current.inventories[(int)GoodsType.common].In(GoodsDictionary.GetRandom(GoodsType.common));
            gameObject.SetActive(false);
        }
    }
}
