using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityAttribute
{
    public int uID;
    public string name;
    public int level;
    public int hP;
    public int attack;
    public int attackCorrection;
    public int defense;
    public int dodge;
    public int hit;
    [Range(0.2f, 4f)]
    public float interval;
}
