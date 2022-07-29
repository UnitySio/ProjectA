using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poolable : MonoBehaviour
{
    protected ObjectPoolManager objectPoolManager;
    protected string key;

    public virtual void Create(ObjectPoolManager objectPoolManager, string key)
    {
        this.objectPoolManager = objectPoolManager;
        this.key = key;
        gameObject.SetActive(false);
    }

    public virtual void Push()
    {
        objectPoolManager.Push(this, key);
    }
}
