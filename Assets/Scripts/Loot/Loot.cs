using UnityEngine;

public class Loot : MonoBehaviour
{
    [HideInInspector]
    public string lootName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag==Tags.Player)
        {
            gameObject.SetActive(false);
            other.gameObject.GetComponent<PlayerInventory>().AddItem(lootName);
        }
    }
}
