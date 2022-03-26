using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : StateMachine
{
    private Transform hPBarChild;

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
    public float interval;

    public void Setup(int no, string name, int level, int hP, int attack, int attackCorrection, int defense, int dodge, int hit, float interval)
    {
        this.no = no;
        this.name = name;
        this.level = level;
        this.HP = hP;
        this.attack = attack;
        this.attackCorrection = attackCorrection;
        this.defense = defense;
        this.dodge = dodge;
        this.hit = hit;
        this.interval = interval;
    }

    public virtual void Hurt(int damage)
    {
        HP -= damage;
        if (hPBarChild != null) hPBarChild.GetComponent<HPBar>().HP -= damage;
        if (HP > 0) Hit(damage);
        else if (HP <= 0) Death();
    }

    public virtual void Hit(int damage)
    {
        FloatingDamage(damage.ToString());
    }

    public abstract void Death();

    public abstract void Victory();

    public abstract void Defeat();

    public void CreateHPBar()
    {
        GameObject hPBar = Instantiate(BattleManager.Instance.hPBar, transform.position, transform.rotation);
        hPBarChild = hPBar.transform.GetChild(2);

        hPBar.transform.SetParent(transform);
        hPBar.transform.localPosition = new Vector3(hPBar.transform.localPosition.x, hPBar.transform.localPosition.y + GetComponent<SpriteRenderer>().bounds.size.y, hPBar.transform.localPosition.z);
        hPBarChild.GetComponent<HPBar>().Setup(HP);
    }

    public void FloatingDamage(string damage)
    {
        GameObject floatingDamage = Instantiate(BattleManager.Instance.floatingDamage, transform.position, transform.rotation);

        floatingDamage.transform.SetParent(transform);
        floatingDamage.transform.localPosition = new Vector3(floatingDamage.transform.localPosition.x, floatingDamage.transform.localPosition.y + GetComponent<SpriteRenderer>().bounds.size.y, floatingDamage.transform.localPosition.z);
        floatingDamage.GetComponent<FloatingDamage>().Setup(damage);
    }
}
