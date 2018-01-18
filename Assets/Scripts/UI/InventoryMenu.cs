using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryMenu : Menu<InventoryMenu>
{
    public Transform content;
    public Sprite gridImage;
    public Sprite[] sprites;
    public ModelDrag modelDrag;

    private PlayerInventory playerInventory;

    private List<Transform> gridList;
    private GameObject currModel;
    private Vector3 modelPos;
    private Quaternion modelRot;

    protected override void Awake()
    {
        base.Awake();

        playerInventory = GameObject.FindGameObjectWithTag(Tags.Player).GetComponent<PlayerInventory>();
        InitialGrid();
        modelPos = new Vector3(0f, 49.24f, 2.24f);
        modelRot = Quaternion.Euler(0, 180, 0);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (currModel != null)
        {
            playerInventory.ShiftModel(currModel.transform);
        }
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void InitialGrid()
    {
        gridList = new List<Transform>();

        int count = playerInventory.ItemList.Count;
        for (int i = 0; i < playerInventory.size; i++)
        {
            var grid = new GameObject("grid");
            var image = grid.AddComponent<Image>();
            image.rectTransform.localScale = new Vector3(1, 1, 1);
            image.sprite = gridImage;
            grid.transform.SetParent(content);

            if (i < count)
            {
                AddItem(grid.transform, playerInventory.ItemList[i]);
            }

            gridList.Add(grid.transform);
        }

    }

    private void AddItem(Transform grid, string name)
    {
        var item = new GameObject("item");
        foreach (var sprite in sprites)
        {
            if (sprite.name == name)
            {
                var image = item.AddComponent<Image>();
                image.sprite = sprite;
                image.rectTransform.anchorMin = Vector2.zero;
                image.rectTransform.anchorMax = new Vector2(1, 1);
                //image.rectTransform.offsetMin = new Vector2(2, 2);
                //image.rectTransform.offsetMax = new Vector2(2, 2);
                item.transform.SetParent(grid);
                item.transform.localPosition = Vector3.zero;
                item.AddComponent<GridItem>();
                break;
            }
        }
    }

    public void SelectItem(string name)
    {
        if (currModel != null)
        {
            DestroyImmediate(currModel);
        }
        var prefab = Resources.Load<GameObject>(name);
        currModel = Instantiate(prefab, modelPos, modelRot);
        currModel.name = name;
        modelDrag.model = currModel.transform;
    }
}
