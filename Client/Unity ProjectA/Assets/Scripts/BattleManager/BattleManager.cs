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

    public List<Entity> friendly = new List<Entity>();
    public List<Entity> enemy = new List<Entity>();

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) Time.timeScale -= 1.0f;
        if (Input.GetKeyDown(KeyCode.W)) Time.timeScale += 1.0f;
        Debug.Log(Time.timeScale);
    }
}
