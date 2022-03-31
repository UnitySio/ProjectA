using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    private static CardManager instance;
    public static CardManager Instance
    {
        get
        {
            if (instance == null) return null;
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
            Destroy(instance);
    }
}
