using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool:MonoBehaviour
{
    public GameObject prefab;

    private Stack<GameObject> poolStack;
    private Action<GameObject> resetAction;
    private Action<GameObject> initialAction;

    public void Initial(int initialSize, Action<GameObject> resetAction=null,Action<GameObject> initialAction=null)
    {
        poolStack = new Stack<GameObject>(initialSize);
        this.resetAction = resetAction;
        this.initialAction = initialAction;
    }

    public GameObject Get()
    {
        if (poolStack.Count>0)
        {
            var obj= poolStack.Pop();
            resetAction?.Invoke(obj);
            return obj;
        }
        else
        {
            var obj = Instantiate(prefab);
            initialAction?.Invoke(obj);
            return obj;
        }
        
    }

    public void Store(GameObject obj)
    {
        poolStack.Push(obj);
    }
}
