using System.Collections.Generic;
using UnityEngine;

public class PlantPool : MonoBehaviour
{
    public GameObject[] trees;
    public GameObject[] bushes;
    public GameObject[] grasses;
    public int initialSize = 120;

    private Stack<GameObject>[] treePool;
    private Stack<GameObject>[] bushPool;
    private Stack<GameObject>[] grassPool;
    private Stack<GameObject>[] treeCache;
    private Stack<GameObject>[] bushCache;
    private Stack<GameObject>[] grassCache;

    private float timeToClear = 3;
    private float timer;
    private bool isCleared;

    public void Initial()
    {
        treePool = new Stack<GameObject>[trees.Length];
        bushPool = new Stack<GameObject>[bushes.Length];
        grassPool = new Stack<GameObject>[grasses.Length];
        treeCache = new Stack<GameObject>[trees.Length];
        bushCache = new Stack<GameObject>[bushes.Length];
        grassCache = new Stack<GameObject>[grasses.Length];

        Initial(trees, treePool, treeCache, initialSize);
        Initial(bushes, bushPool, bushCache, initialSize);
        Initial(grasses, grassPool, grassCache, initialSize * 50);

        timer = timeToClear;
    }

    /// <summary>
    /// 初始化植物，将其放入cache中，准备首次加载场景时使用
    /// </summary>
    /// <param name="plants"></param>
    /// <param name="pool"></param>
    /// <param name="cache"></param>
    /// <param name="size"></param>
    private void Initial(GameObject[] plants, Stack<GameObject>[] pool, Stack<GameObject>[] cache, int size)
    {
        for (int i = 0; i < plants.Length; i++)
        {
            pool[i] = new Stack<GameObject>();
            cache[i] = new Stack<GameObject>();
            for (int j = 0; j < size; j++)
            {
                var plant = Instantiate(plants[i]);
                plant.name = plants[i].name;
                cache[i].Push(plant);
            }
        }
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
        timer = timeToClear;
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
        timer = timeToClear;
        return result;
    }

    public GameObject GetGrass()
    {
        var cache = grassCache[0];

        GameObject result = null;
        if (cache.Count > 0)
        {
            result = cache.Pop();
        }
        else
        {
            var pool = grassPool[0];
            if (pool.Count > 0)
            {
                result = pool.Pop();
                result.SetActive(true);
            }
            else
            {
                result = Instantiate(grasses[0]);
                result.name = grasses[0].name;
            }
        }
        timer = timeToClear;
        return result;
    }

    public void ReuseInCache(GameObject plant)
    {
        //plant.transform.SetParent(transform); //从terrain的子物体转移，防止删除terrain时被删除
        for (int i = 0; i < grasses.Length; i++)
        {
            if (plant.name == grasses[i].name)
            {
                grassCache[i].Push(plant);
                return;
            }
        }
        for (int i = 0; i < trees.Length; i++)
        {
            if (plant.name == trees[i].name)
            {
                treeCache[i].Push(plant);
                return;
            }
        }
        for (int i = 0; i < bushes.Length; i++)
        {
            if (plant.name == bushes[i].name)
            {
                bushCache[i].Push(plant);
                return;
            }
        }
    }

    public void ClearCahce()
    {
        ClearCache(treeCache, treePool);
        ClearCache(bushCache, bushPool);
        ClearCache(grassCache, grassPool);
    }

    private void ClearCache(Stack<GameObject>[] plantCache, Stack<GameObject>[] plantPool)
    {
        if (plantCache == null || plantPool == null)
        {
            return;
        }
        for (int i = 0; i < plantCache.Length; i++)
        {
            var cache = plantCache[i];
            var pool = plantPool[i];
            while (cache.Count > 0)
            {
                var plant = cache.Pop();
                plant.SetActive(false);
                //plant.transform.SetParent(transform);
                pool.Push(plant);
            }
        }
    }

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            isCleared = false;
        }
        else if (!isCleared)
        {
            ClearCahce();
            isCleared = true;
        }
    }
}
