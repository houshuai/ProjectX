﻿using System.Collections.Generic;
using UnityEngine;

public class LootController : MonoBehaviour
{
    public GameObject lootPrefab;
    public int initialCount;

    [HideInInspector]
    public static LootController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LootController>();
            }
            if (instance == null)
            {
                Debug.Log("not found LootController");
            }
            return instance;
        }
    }

    private static LootController instance;
    private List<GameObject> allLoot;

    private void Start()
    {
        allLoot = new List<GameObject>();

        for (int i = 0; i < initialCount; i++)
        {
            var loot = Instantiate(lootPrefab);
            loot.SetActive(false);
            allLoot.Add(loot);
        }
    }

    public void GetLoot(int count, Vector3 pos)
    {
        int currAvailable = 0;
        foreach (var loot in allLoot)
        {
            if (!loot.gameObject.activeSelf)
            {
                var circle = Random.insideUnitCircle;
                loot.transform.position = new Vector3(pos.x + circle.x, pos.y, pos.z + circle.y);
                loot.SetActive(true);
                currAvailable++;
            }
            if (currAvailable == count)
            {
                return;
            }
        }

        int less = count - currAvailable;
        for (int i = 0; i < less; i++)
        {
            var circle = Random.insideUnitCircle;
            var loot = Instantiate(lootPrefab,
                new Vector3(pos.x + circle.x, pos.y, pos.z + circle.y), Quaternion.identity);
            allLoot.Add(loot);
        }
    }

}
