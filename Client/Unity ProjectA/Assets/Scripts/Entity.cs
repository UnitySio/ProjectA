using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : StateMachine
{
    public int no;
    public string name;
    public int level;

    [SerializeField]
    private int hP;
    public int HP
    {
        get { return hP; }
        set { hP = value; }
    }

    public int attack;
    public int attackCorrection;
    public int defense;
    public int dodge;
    public int hit;
    [Range(0.2f, 4f)]
    public float actionInterval;

    public void Setup()
    {
        this.no = 1;
        this.name = "Entity";
        this.level = 1;
        this.HP = 100;
        this.attack = 1;
        this.attackCorrection = 5;
        this.defense = 100;
        this.dodge = 10;
        this.hit = 10;
        this.actionInterval = 1f;
    }

    public virtual void Hurt(int damage)
    {
        HP -= damage;
        if (HP > 0) Hit();
        else if (HP <= 0) Death();
    }

    public abstract void Hit();
    public abstract void Death();
}
