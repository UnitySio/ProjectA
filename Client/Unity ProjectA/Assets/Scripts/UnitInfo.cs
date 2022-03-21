using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInfo : MonoBehaviour
{
    public int no;
    public int level;
    public int hp;
    public int attack;
    public int attackCorrection;
    public int defense;
    public int dodge;
    public int hit;
    [Range(0.2f, 4f)]
    public float actionInterval;
}
