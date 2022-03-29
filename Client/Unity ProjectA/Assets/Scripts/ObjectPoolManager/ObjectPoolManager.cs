using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [System.Serializable]
    public class pool
    {
        public string key;
        public Poolable prefab;
        public int count;
    }

    private static ObjectPoolManager instance;
    public static ObjectPoolManager Instance
    {
        get
        {
            if (instance == null) return null;
            return instance;
        }
    }

    public List<pool> pools = new List<pool>();
    public Dictionary<string, Stack<Poolable>> poolDictionary = new Dictionary<string, Stack<Poolable>>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        foreach (pool pool in pools)
        {
            Stack<Poolable> poolStack = new Stack<Poolable> ();
            for (int i = 0; i < pool.count; i++)
            {
                Poolable poolable = Instantiate(pool.prefab, transform);
                poolable.Create(this, pool.key);
                poolStack.Push(poolable);
            }

            poolDictionary.Add(pool.key, poolStack);
        }
    }

    public GameObject Pop(string key, Entity owner)
    {
        Poolable poolable = poolDictionary[key].Pop();
        poolable.transform.SetParent(owner.transform);
        poolable.gameObject.SetActive(true);
        return poolable.gameObject;
    }

    public void Push(Poolable poolable, string key)
    {
        poolable.gameObject.SetActive(false);
        poolable.transform.SetParent(transform);
        poolDictionary[key].Push(poolable);
    }
}
