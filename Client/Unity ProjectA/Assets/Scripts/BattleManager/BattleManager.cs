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

    public bool isVictory;
    public bool isDefeat;

    public List<Entity> friendly = new List<Entity>();
    public List<Entity> enemy = new List<Entity>();

    public GameObject floatingDamage;
    public GameObject hPBar;

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

    private void Update()
    {
        if (isVictory != true || isDefeat != true)
            if (enemy.Count == 0)
                isVictory = true;
            else if (friendly.Count == 0)
                isDefeat = true;
    }
}
