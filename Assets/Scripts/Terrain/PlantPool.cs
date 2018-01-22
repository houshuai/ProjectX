using System.Collections.Generic;
using UnityEngine;

public class PlantPool : MonoBehaviour
{
    public GameObject[] trees;
    public GameObject[] bushes;
    public int initialSize = 120;

    private Stack<GameObject>[] treePool;
    private Stack<GameObject>[] bushPool;
    private Stack<GameObject>[] treeCache;
    private Stack<GameObject>[] bushCache;

    private float timer;
    private bool isCleared;

    public void InitialPool()
    {
        treePool = new Stack<GameObject>[trees.Length];
        bushPool = new Stack<GameObject>[bushes.Length];
        treeCache = new Stack<GameObject>[trees.Length];
        bushCache = new Stack<GameObject>[bushes.Length];

        for (int i = 0; i < trees.Length; i++)
        {
            treePool[i] = new Stack<GameObject>();
            treeCache[i] = new Stack<GameObject>();
            for (int j = 0; j < initialSize; j++)
            {
                var tree = Instantiate(trees[i]);
                tree.name = trees[i].name;
                treeCache[i].Push(tree);
            }
        }

        for (int i = 0; i < bushes.Length; i++)
        {
            bushPool[i] = new Stack<GameObject>();
            bushCache[i] = new Stack<GameObject>();
            for (int j = 0; j < initialSize; j++)
            {
                var bush = Instantiate(bushes[i]);
                bush.name = bushes[i].name;
                bushCache[i].Push(bush);
            }
        }

        timer = 1;
    }

    public GameObject GetTree(int index)
    {
        var cache = treeCache[index];

        GameObject result = null;
        if (cache.Count > 0)
        {
            result = cache.Pop();
        }
        else
        {
            var pool = treePool[index];
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
        }
        timer = 1;
        return result;
    }

    public GameObject GetBush(float percent)
    {
        int index = (int)(percent * bushPool.Length);
        var cache = bushCache[index];

        GameObject result = null;
        if (cache.Count > 0)
        {
            result = cache.Pop();
        }
        else
        {
            var pool = bushPool[index];
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
        }
        timer = 1;
        return result;
    }

    public void ReuseInCache(GameObject plant)
    {
        for (int i = 0; i < trees.Length; i++)
        {
            if (plant.name == trees[i].name)
            {
                treeCache[i].Push(plant);
            }
        }
        for (int i = 0; i < bushes.Length; i++)
        {
            if (plant.name == bushes[i].name)
            {
                bushCache[i].Push(plant);
            }
        }
    }

    public void ClearCahce()
    {
        ClearCache(treeCache, treePool);
        ClearCache(bushCache, bushPool);
    }

    private void ClearCache(Stack<GameObject>[] plantCache, Stack<GameObject>[] plantPool)
    {
        for (int i = 0; i < plantCache.Length; i++)
        {
            var cache = plantCache[i];
            var pool = plantPool[i];
            while (cache.Count > 0)
            {
                var plant = cache.Pop();
                plant.SetActive(false);
                plant.transform.SetParent(transform);
                pool.Push(plant);
            }
        }
    }

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else if (!isCleared)
        {
            ClearCahce();
            isCleared = true;
        }
    }
}
