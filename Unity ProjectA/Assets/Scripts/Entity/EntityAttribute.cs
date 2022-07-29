using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityAttribute
{
    public int uid;
    public string name;
    public int lv;
    public int hp;
    public int attack;
    public int defense;
    public int dodge;
    public int hit;
    [Range(0.2f, 4f)]
    public float interval;
}
