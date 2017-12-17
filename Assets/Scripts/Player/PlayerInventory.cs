using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int size = 12;
    public Inventory inventory;
    [HideInInspector]
    public List<string> ItemList { get; private set; }

    private PlayerMove playerMove;
    private PlayerHealth playerHealth;
    private PlayerAttack playerAttack;
    private GameObject currItem;

    private void Awake()
    {
        inventory = GameController.Instance.currArchive.inventory;
        ItemList = new List<string>();
        playerMove = GetComponent<PlayerMove>();
        playerHealth = GetComponent<PlayerHealth>();
        playerAttack = GetComponent<PlayerAttack>();
        currItem = GetComponentInChildren<PlayAnimationEvent>().gameObject;
    }

    public void AddItem(string name)
    {
        if (ItemList.Count>12)
        {
            return;
        }

        ItemList.Add(name);
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
        if (instance!=null)
        {
            instance.BeforeUnload -= Save;
            instance.AfterLoaded -= Load;
        }
    }

    private void Save()
    {
        inventory.itemList.Clear();
        inventory.itemList.AddRange(ItemList);
        inventory.current = currItem.name;
    }

    private void Load()
    {
        ItemList.Clear();
        ItemList.AddRange(inventory.itemList);
        string currName = inventory.current;
        if (currName != "")
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
    }
}
