using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    private PlayerMove playerMove;
    private PlayerHealth playerHealth;
    private PlayerAttack playerAttack;
    private GameObject currItem;

    private void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        playerHealth = GetComponent<PlayerHealth>();
        playerAttack = GetComponent<PlayerAttack>();
        currItem = GetComponentInChildren<PlayAnimationEvent>().gameObject;
        GameController.Instance.player = transform;
    }

    private void OnEnable()
    {
        var instance = Scene.Instance;
        if (instance != null)
        {
            instance.AfterLoaded += Load;
        }
    }

    private void OnDisable()
    {
        var instance = Scene.Instance;
        if (instance != null)
        {
            instance.AfterLoaded -= Load;
        }
    }

    private void Load()
    {
        var currGoods = Archive.current.currGoods;
        if (currGoods != null)
        {
            var prefab = Resources.Load<GameObject>(currGoods.name);
            if (prefab!=null)
            {
                var newModel = Instantiate(prefab);
                newModel.name = currGoods.name;
                ShiftModel(newModel.transform, currGoods);
            }
        }

    }

    public void ShiftModel(Transform newModel, Goods goods)
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
        Archive.current.currGoods = goods;
    }
}
