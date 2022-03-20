using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BattleManager : MonoBehaviour
{
    private static BattleManager instance;
    public static BattleManager Instance
    {
        get
        {
            if (instance == null) return null;
            return instance;
        }
    }

    public List<EntityInfo> friendly = new List<EntityInfo>();
    public List<EntityInfo> enemy = new List<EntityInfo>();

    public List<GameObject> friendlyList = new List<GameObject>();
    public List<GameObject> enemyList = new List<GameObject>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this) Destroy(gameObject);
    }

    private void Start()
    {

    }
}
