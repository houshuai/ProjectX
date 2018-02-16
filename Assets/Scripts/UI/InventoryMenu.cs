using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryMenu : Menu<InventoryMenu>
{
    public Transform content;
    public Sprite gridImage;
    public Sprite[] sprites;
    public Font font;
    public ModelDrag modelDrag;
    public GameObject deleteUI;

    private PlayerEquipment player;
    private Inventory inventory;
    private List<GameObject> gridList;
    private GameObject currModel;
    private Goods currGoods;
    private Vector3 modelPos;
    private Quaternion modelRot;
    private bool isDelete;
    private OverlayGoods overlayToDelete;
    private InputField input;

    protected override void Awake()
    {
        base.Awake();

        player = GameObject.FindObjectOfType<PlayerEquipment>();
        InitialGrid();
        modelPos = new Vector3(0f, 49.24f, 2.24f);
        modelRot = Quaternion.Euler(0, 180, 0);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (currModel != null)
        {
            player.ShiftModel(currModel.transform, currGoods);
        }
    }

    private void InitialGrid()
    {
        inventory = Archive.current.inventory;
        gridList = new List<GameObject>(inventory.capacity);
        for (int i = 0; i < inventory.capacity; i++)
        {
            var grid = new GameObject("grid");
            var image = grid.AddComponent<Image>();
            image.rectTransform.localScale = new Vector3(1, 1, 1);
            image.sprite = gridImage;
            grid.transform.SetParent(content);

            if (i < inventory.itemList.Count)
            {
                AddItem(grid.transform, inventory.itemList[i]);
            }
            gridList.Add(grid);
        }

    }

    /// <summary>
    /// 生成物品的显示界面
    /// </summary>
    /// <param name="grid">物品所在的父网格</param>
    /// <param name="overlay">物品和其数量</param>
    private void AddItem(Transform grid, OverlayGoods overlay)
    {
        var item = new GameObject("item");
        var image = item.AddComponent<Image>();
        image.sprite = sprites[overlay.goods.id];
        item.transform.SetParent(grid);
        item.transform.localPosition = Vector3.zero;
        item.AddComponent<GridItem>().overlay = overlay;
        image.rectTransform.anchorMin = new Vector2(0, 0.2f);
        image.rectTransform.anchorMax = new Vector2(1, 1);
        image.rectTransform.sizeDelta = Vector2.zero;

        var count = new GameObject("count");
        var text = count.AddComponent<Text>();
        text.text ="x" + overlay.count.ToString();
        text.transform.SetParent(grid);
        text.transform.localPosition = Vector3.zero;
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = new Vector2(1, 0.2f);
        text.rectTransform.sizeDelta = Vector2.zero;
        text.font = font;
        text.fontSize = 20;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.red;
    }

    public void GridItemClick(OverlayGoods overlay)
    {
        if (isDelete)
        {
            overlayToDelete = overlay;
            input.text = "1";
        }
        else
        {
            SelectItem(overlay.goods);
        }
    }

    public void Delete()
    {
        deleteUI.SetActive(true);
        input = deleteUI.GetComponentInChildren<InputField>();
        input.text = "";
        isDelete = true;
    }

    public void Decrease()
    {
        var curr = int.Parse(input.text);
        if (curr > 1)
        {
            input.text = (curr - 1).ToString();
        }
    }

    public void Increase()
    {
        var curr = int.Parse(input.text);
        if (curr<overlayToDelete.count)
        {
            input.text = (curr + 1).ToString();   
        }
    }

    public void Yes()
    {
        if (overlayToDelete!=null)
        {
            DeleteItem();
        }
        isDelete = false;
        deleteUI.SetActive(false);
        overlayToDelete = null;
    }

    public void Cancel()
    {
        isDelete = false;
        deleteUI.SetActive(false);
        overlayToDelete = null;
    }

    private void SelectItem(Goods goods)
    {
        if (currModel != null)
        {
            DestroyImmediate(currModel);
        }
        var prefab = Resources.Load<GameObject>(goods.name);
        currModel = Instantiate(prefab, modelPos, modelRot);
        modelDrag.model = currModel.transform;
        currGoods = goods;
    }

    private void DeleteItem()
    {
        var deleteCount = int.Parse(input.text);
        for (int i = 0; i < deleteCount; i++)
        {
            inventory.Out(overlayToDelete.goods);
        }
        
        for (int i = 0; i < gridList.Count; i++)
        {
            var item = gridList[i].GetComponentInChildren<GridItem>();
            if (item!=null)
            {
                Destroy(item.gameObject);
                Destroy(item.transform.parent.GetComponentInChildren<Text>().gameObject);
            }
            
        }

        for (int i = 0; i < gridList.Count; i++)
        {
            if (i < inventory.itemList.Count)
            {
                AddItem(gridList[i].transform, inventory.itemList[i]);
            }
        }
    }
}
