using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField]
    private float hp;
    public float HP
    {
        get { return hp; }
        set
        {
            hp = value;
            if (hp < 0) hp = 0;
        }
    }

    public float maxHP;
    private float origin;
    private float offset;

    public GameObject hpBar;
    public GameObject hpBarMask;

    private void Awake()
    {
        origin = hpBarMask.transform.localScale.x;
    }

    private void Start()
    {
        if (gameObject.transform.parent.CompareTag("Friendly"))
            hpBar.GetComponent<SpriteRenderer>().color = Color.green;
        else if (gameObject.transform.parent.CompareTag("Enemy"))
            hpBar.GetComponent<SpriteRenderer>().color = Color.red;
    }

    private void Update()
    {
        offset = (hp / maxHP) * origin;
        hpBarMask.transform.localScale = new Vector3(offset, hpBarMask.transform.localScale.y, hpBarMask.transform.localScale.z);
    }

    public void Setup(int hP)
    {
        this.hp = hP;
        this.maxHP = hP;
    }
}
