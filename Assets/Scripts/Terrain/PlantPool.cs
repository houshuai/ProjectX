using System.Collections.Generic;
using UnityEngine;

public class PlantPool : MonoBehaviour
{
    public GameObject[] trees;
    public GameObject[] bushes;
    public int initialSize = 120;

    [HideInInspector]
    public Stack<GameObject>[] treePool;
    [HideInInspector]
    public Stack<GameObject>[] bushPool;

    public void InitialPool()
    {
        treePool = new Stack<GameObject>[trees.Length];
        for (int i = 0; i < trees.Length; i++)
        {
            treePool[i] = new Stack<GameObject>();
            for (int j = 0; j < initialSize; j++)
            {
                var tree = Instantiate(trees[i]);
                tree.name = trees[i].name;
                tree.transform.SetParent(transform, false);
                tree.SetActive(false);
                treePool[i].Push(tree);
            }
        }

        bushPool = new Stack<GameObject>[bushes.Length];
        for (int i = 0; i < bushes.Length; i++)
        {
            bushPool[i] = new Stack<GameObject>();
            for (int j = 0; j < initialSize; j++)
            {
                var bush = Instantiate(bushes[i]);
                bush.name = bushes[i].name;
                bush.transform.SetParent(transform, false);
                bush.SetActive(false);
                bushPool[i].Push(bush);
            }
        }
    }

    public GameObject GetTree(int index)
    {
        var pool = treePool[index];

        GameObject result = null;
        if (pool.Count > 0)
        {
            result = pool.Pop();
            result.SetActive(true);
        }
        else
        {
            result = Instantiate(trees[index]);
            result.name = trees[index].name;
        }

        return result;
    }

    public GameObject GetBush(float percent)
    {
        int index = (int)(percent * bushPool.Length);
        var pool = bushPool[index];

        GameObject result = null;
        if (pool.Count > 0)
        {
            result = pool.Pop();
            result.SetActive(true);
        }
        else
        {
            result = Instantiate(bushes[index]);
            result.name = bushes[index].name;
        }

        return result;
    }

    public void Reuse(GameObject plant)
    {
        plant.SetActive(false);
        plant.transform.SetParent(transform, false);
        for (int i = 0; i < trees.Length; i++)
        {
            if (plant.name == trees[i].name)
            {
                treePool[i].Push(plant);
            }
        }
        for (int i = 0; i < bushes.Length; i++)
        {
            if (plant.name == bushes[i].name)
            {
                bushPool[i].Push(plant);
            }
        }
    }
}
