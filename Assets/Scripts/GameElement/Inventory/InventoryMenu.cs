using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryMenu : Menu<InventoryMenu>
{
    public Text gemText;
    public Transform commonContent;
    public Transform clothContent;
    public Sprite gridImage;
    public Sprite[] sprites;
    public GameObject commonScrollview;
    public GameObject clothScrolliew;
    public GameObject sellButton;
    public Font font;
    public ModelDrag modelDrag;
    public GameObject sellUI;
    public GameObject buyUI;
    public GameObject dressUI;
    public Text toolTip;
    public Text toolTipContent;

    private PlayerEquipment player;
    private Inventory[] inventories;
    private GameObject currModel;              //当前衣服的预览模型
    private Goods currGoods;                   //当前的衣服
    private Vector3 modelPos;                  //实例化预览模型的位置
    private Quaternion modelRot;               //模型的旋转
    private bool isDelete;                     //是否在进行删除物品操作
    private OverlayGoods selectedOverlay;      //选中的物品
    private Text deleteNumText;                //被删除的物品数量text
    private Text goodsPriceText;               //物品的价格text
    private bool isClothConentInitialized;

    protected override void Awake()
    {
        base.Awake();

        player = FindObjectOfType<PlayerEquipment>();

        inventories = Archive.current.inventories;
        InitialGrid(commonContent, inventories[(int)GoodsType.common]);
        gemText.text = Archive.current.gemCount.ToString();

        modelPos = new Vector3(0f, 49.24f, 2.24f);
        modelRot = Quaternion.Euler(0, 180, 0);
    }

    /// <summary>
    /// 初始化背包界面
    /// </summary>
    private void InitialGrid(Transform content, Inventory inventory)
    {
        for (int i = 0; i < inventory.capacity; i++)
        {
            var grid = new GameObject("grid");
            grid.transform.SetParent(content);
            grid.transform.localPosition = Vector3.zero;
            grid.transform.localScale = Vector3.one;
            var image = grid.AddComponent<Image>();
            image.rectTransform.localScale = new Vector3(1, 1, 1);
            image.sprite = gridImage;

            if (i < inventory.itemList.Count)
            {
                AddItem(grid.transform, inventory.itemList[i]);
            }
        }

    }

    /// <summary>
    /// 生成物品的显示界面
    /// </summary>
    /// <param name="grid">物品所在的父网格</param>
    /// <param name="overlay">物品</param>
    private void AddItem(Transform grid, OverlayGoods overlay)
    {
        var item = new GameObject("item");
        item.transform.SetParent(grid);
        item.transform.localPosition = Vector3.zero;
        var gridItem = item.AddComponent<GridItem>();
        gridItem.overlay = overlay;
        gridItem.toolTip = toolTip;
        gridItem.content = toolTipContent;
        var image = item.AddComponent<Image>();
        image.sprite = sprites[overlay.goods.id];
        image.rectTransform.localScale = Vector3.one;
        image.rectTransform.anchorMin = new Vector2(0, 0.2f);
        image.rectTransform.anchorMax = new Vector2(1, 1);
        image.rectTransform.sizeDelta = Vector2.zero;

        var count = new GameObject("count");
        count.transform.SetParent(grid);
        count.transform.localPosition = Vector3.zero;
        var text = count.AddComponent<Text>();
        text.text = "x" + overlay.count.ToString();
        text.rectTransform.localScale = Vector3.one;
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = new Vector2(1, 0.2f);
        text.rectTransform.sizeDelta = Vector2.zero;
        text.font = font;
        text.fontSize = 20;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.red;
    }

    /// <summary>
    /// 点击切换到普通物品界面按钮
    /// </summary>
    public void CommonBtnClick()
    {
        if (!commonScrollview.activeSelf)
        {
            clothScrolliew.SetActive(false);
            buyUI.SetActive(false);
            dressUI.SetActive(false);
            commonScrollview.SetActive(true);
            sellButton.SetActive(true);
        }
    }

    /// <summary>
    /// 点击切换到衣服界面按钮
    /// </summary>
    public void ClothBtnClick()
    {
        if (!clothScrolliew.activeSelf)
        {
            commonScrollview.SetActive(false);
            sellButton.SetActive(false);
            clothScrolliew.SetActive(true);
            CancelBtnClick();
            if (!isClothConentInitialized)
            {
                var inventory = new Inventory(12, 1);
                var allCloth = GoodsDictionary.Get(GoodsType.cloth);
                foreach (var cloth in allCloth)
                {
                    inventory.In(cloth);
                }
                InitialGrid(clothContent, inventory);

                var items = clothContent.GetComponentsInChildren<GridItem>();
                var itemList = inventories[(int)GoodsType.cloth].itemList;
                //对于展示的所有衣服
                foreach (var item in items)
                {
                    bool haveColth = false;
                    //遍历背包中的衣服
                    foreach (var itemInInventory in itemList)
                    {
                        if (item.overlay.goods == itemInInventory.goods)  //一定要判断goods的重载了==，overlay的地址不一样
                        {
                            haveColth = true;
                        }
                    }
                    //如果没有这件，就将其颜色设置为灰色
                    if (!haveColth)
                    {
                        item.gameObject.GetComponent<Image>().color = Color.gray;
                    }
                }
                isClothConentInitialized = true;
            }
        }
    }

    /// <summary>
    /// 点击卖出按钮
    /// </summary>
    public void SellBtnClick()
    {
        if (commonScrollview.activeSelf)
        {
            sellUI.SetActive(true);
            var texts = sellUI.GetComponentsInChildren<Text>();
            deleteNumText = texts[1];
            goodsPriceText = texts[3];
            deleteNumText.text = "0";
            goodsPriceText.text = "0";
            isDelete = true;
        }
    }

    /// <summary>
    /// 点击了减少按钮
    /// </summary>
    public void DecreaseBtnClick()
    {
        var curr = int.Parse(deleteNumText.text);
        if (curr > 1)
        {
            deleteNumText.text = (curr - 1).ToString();
            goodsPriceText.text = ((curr - 1) * selectedOverlay.goods.price).ToString();
        }
    }

    /// <summary>
    /// 点击了增加按钮
    /// </summary>
    public void IncreaseBtnClick()
    {
        var curr = int.Parse(deleteNumText.text);
        if (curr < selectedOverlay.count)
        {
            deleteNumText.text = (curr + 1).ToString();
            goodsPriceText.text = ((curr + 1) * selectedOverlay.goods.price).ToString();
        }
    }

    /// <summary>
    /// 点击了确定按钮
    /// </summary>
    public void YesBtnClick()
    {
        if (selectedOverlay != null)
        {
            SellGoods(selectedOverlay.goods);
        }
        isDelete = false;
        sellUI.SetActive(false);
        selectedOverlay = null;
    }

    /// <summary>
    /// 点击了取消按钮
    /// </summary>
    public void CancelBtnClick()
    {
        isDelete = false;
        sellUI.SetActive(false);
        selectedOverlay = null;
    }

    /// <summary>
    /// 点击了购买衣服按钮
    /// </summary>
    public void BuyBtnClick()
    {
        BuyGoods(selectedOverlay.goods);
        buyUI.SetActive(false);
    }

    /// <summary>
    /// 点击了换装按钮
    /// </summary>
    public void DressBtnClick()
    {
        if (currModel != null)
        {
            player.ShiftModel(currModel.transform, currGoods);
        }

        //当前选择的模型设置为null，避免关闭背包的时候将其destroy
        currModel = null;
        dressUI.SetActive(false);
        Back();
    }

    /// <summary>
    /// 点击了网格里的物品
    /// </summary>
    /// <param name="overlay"></param>
    public void GridItemClick(OverlayGoods overlay)
    {
        selectedOverlay = overlay;
        if (isDelete && overlay.goods.type == GoodsType.common)
        {
            deleteNumText.text = "1";
            goodsPriceText.text = overlay.goods.price.ToString();
        }
        else if (overlay.goods.type == GoodsType.cloth)
        {
            //遍历衣服背包
            foreach (var item in inventories[(int)GoodsType.cloth].itemList)
            {
                //如果背包中有这件衣服，就可以选择
                if (overlay.goods == item.goods)
                {
                    SelectCloth(overlay.goods);
                    if (buyUI.activeSelf)
                    {
                        buyUI.SetActive(false);
                    }
                    if (!dressUI.activeSelf)
                    {
                        dressUI.SetActive(true);
                    }
                    return;
                }
            }

            //如果没有这件衣服
            if (dressUI.activeSelf)
            {
                dressUI.SetActive(false);
            }
            if (!buyUI.activeSelf)
            {
                buyUI.SetActive(true);
            }
            buyUI.GetComponentInChildren<Text>().text = overlay.goods.price.ToString();
        }
    }

    /// <summary>
    /// 选中了衣服，将其模型预览出来
    /// </summary>
    /// <param name="goods"></param>
    private void SelectCloth(Goods goods)
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

    /// <summary>
    /// 卖出背包里的物品
    /// </summary>
    private void SellGoods(Goods goods)
    {
        var inventory = inventories[(int)goods.type];
        var sellCount = int.Parse(deleteNumText.text);
        for (int i = 0; i < sellCount; i++)
        {
            if (inventory.Out(goods.id) != null)
            {
                Archive.current.gemCount += goods.price;
            }
        }
        gemText.text = Archive.current.gemCount.ToString();

        //将背包网格的显示都删除
        var itemList = commonContent.GetComponentsInChildren<GridItem>();
        var gridList = new List<Transform>();
        for (int i = 0; i < itemList.Length; i++)
        {
            var item = itemList[i];
            gridList.Add(item.transform.parent);
            Destroy(item.transform.parent.GetComponentInChildren<Text>().gameObject);
            Destroy(item.gameObject);

        }

        //重新创建背包网格
        for (int i = 0; i < gridList.Count; i++)
        {
            if (i < inventory.itemList.Count)
            {
                AddItem(gridList[i], inventory.itemList[i]);
            }
        }
    }

    /// <summary>
    /// 购买物品
    /// </summary>
    /// <param name="goods"></param>
    private void BuyGoods(Goods goods)
    {
        if (Archive.current.gemCount < goods.price)
        {
            return;
        }

        Archive.current.gemCount -= goods.price;
        inventories[(int)goods.type].In(goods);

        gemText.text = Archive.current.gemCount.ToString();

        //将够买的颜色设置为白色
        var grids = clothContent.GetComponentsInChildren<GridItem>();
        foreach (var grid in grids)
        {
            if (grid.overlay.goods == goods)
            {
                grid.gameObject.GetComponent<Image>().color = Color.white;
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        currGoods = null;
        if (currModel != null)
        {
            Destroy(currModel);
            currModel = null;
        }
    }
}
