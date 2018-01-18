using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int size = 12;
    public Inventory inventory;
    [HideInInspector]
    public List<string> ItemList { get { return inventory.itemList; } }

    private PlayerMove playerMove;
    private PlayerHealth playerHealth;
    private PlayerAttack playerAttack;
    private GameObject currItem;

    private void Awake()
    {
        inventory = Archive.current.inventory;
        playerMove = GetComponent<PlayerMove>();
        playerHealth = GetComponent<PlayerHealth>();
        playerAttack = GetComponent<PlayerAttack>();
        currItem = GetComponentInChildren<PlayAnimationEvent>().gameObject;
    }

    public void AddItem(string name)
    {
        if (inventory.itemList.Count > 12)
        {
            return;
        }

        inventory.itemList.Add(name);
    }

    private void OnEnable()
    {
        var instance = Scene.Instance;
        if (instance != null)
        {
            instance.BeforeUnload += Save;
            instance.AfterLoaded += Load;
        }
    }

    private void OnDisable()
    {
        var instance = Scene.Instance;
        if (instance != null)
        {
            instance.BeforeUnload -= Save;
            instance.AfterLoaded -= Load;
        }
    }

    private void Save()
    {
        inventory.current = currItem.name;
    }

    private void Load()
    {
        string currName = inventory.current;
        if (currName != null)
        {
            var prefab = Resources.Load<GameObject>(currName);
            var newModel = Instantiate(prefab);
            newModel.name = currName;
            ShiftModel(newModel.transform);
        }

    }

    public void ShiftModel(Transform newModel)
    {
        Destroy(currItem);
        newModel.position = transform.position;
        newModel.rotation = transform.rotation;
        newModel.gameObject.AddComponent<PlayAnimationEvent>();
        newModel.SetParent(transform);
        var anim = newModel.GetComponent<Animator>();
        playerMove.anim = anim;
        playerHealth.anim = playerMove.anim;
        playerAttack.anim = anim;
        currItem = newModel.gameObject;
        inventory.current = currItem.name;
    }
}
