using UnityEngine;

public class Loot : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == Tags.Player)
        {
            var rand = Random.Range(0, GoodsDictionary.Count);
            gameObject.SetActive(false);
            Archive.current.inventory.In(GoodsDictionary.Get(rand));
        }
    }
}
