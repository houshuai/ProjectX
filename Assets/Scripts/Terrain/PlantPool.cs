using System.Collections.Generic;
using UnityEngine;

public class PlantPool : MonoBehaviour
{
    public GameObject[] trees;
    public GameObject[] bushes;
    public int initialSize = 120;

    [HideInInspector]
    public List<GameObject> tree0Pool;
    [HideInInspector]
    public List<GameObject> tree1Pool;
    [HideInInspector]
    public List<GameObject>[] bushPool;

    public void InitialPool()
    {
        tree0Pool = new List<GameObject>();
        for (int i = 0; i < initialSize; i++)
        {
            var tree = Instantiate(trees[0]);
            tree.transform.SetParent(transform, false);
            tree.SetActive(false);
            tree0Pool.Add(tree);
        }

        tree1Pool = new List<GameObject>();
        for (int i = 0; i < initialSize; i++)
        {
            var tree = Instantiate(trees[1]);
            tree.transform.SetParent(transform, false);
            tree.SetActive(false);
            tree1Pool.Add(tree);
        }

        bushPool = new List<GameObject>[bushes.Length];
        for (int i = 0; i < bushes.Length; i++)
        {
            bushPool[i] = new List<GameObject>();
            for (int j = 0; j < initialSize; j++)
            {
                var bush = Instantiate(bushes[i]);
                bush.transform.SetParent(transform, false);
                bush.SetActive(false);
                bushPool[i].Add(bush);
            }
        }
    }

    public GameObject GetTree(int index)
    {
        List<GameObject> treePool = null;
        if (index == 0)
        {
            treePool = tree0Pool;
        }
        else if (index == 1)
        {
            treePool = tree1Pool;
        }
        else
        {
            throw new KeyNotFoundException("not found tree index");
        }

        GameObject result = null;
        foreach (var tree in treePool)
        {
            if (!tree.activeSelf)
            {
                result = tree;
                result.SetActive(true);
                break;
            }
        }

        if (result == null)
        {
            result = Instantiate(trees[index]);
            treePool.Add(result);
        }

        return result;
    }

    public GameObject GetBush(float percent)
    {
        int index = (int)(percent * bushPool.Length);
        var bushPoolIndex = bushPool[index];

        GameObject result = null;
        foreach (var bush in bushPoolIndex)
        {
            if (!bush.activeSelf)
            {
                result = bush;
                result.SetActive(true);
                break;
            }
        }

        if (result == null)
        {
            result = Instantiate(bushes[index]);
            bushPoolIndex.Add(result);
        }

        return result;
    }
}
