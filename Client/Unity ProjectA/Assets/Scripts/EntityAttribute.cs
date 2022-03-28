using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityAttribute
{
    public string name;
    public int hP, attack, attackCorrection, defense, dodge, hit, no, level;
    [Range(0.2f, 4f)]
    public float interval;
}
