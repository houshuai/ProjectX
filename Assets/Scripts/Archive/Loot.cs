using UnityEngine;

public class Loot : MonoBehaviour
{
    [HideInInspector]
    public int id;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == Tags.Player)
        {
            gameObject.SetActive(false);
            Archive.current.inventory.In(GoodsDictionary.Get(id));
        }
    }
}
