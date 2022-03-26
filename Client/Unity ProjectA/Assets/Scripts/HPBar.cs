using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField]
    private float hP;
    public float HP
    {
        get { return hP; }
        set
        {
            hP = value;

            if (hP < 0) hP = 0;
        }
    }

    public float maxHP;
    private float origin;
    private float offset;

    private void Awake()
    {
        origin = transform.localScale.x;
    }

    private void Update()
    {
        offset = (hP / maxHP) * origin;
        transform.localScale = new Vector3(offset, transform.localScale.y, transform.localScale.z);
    }

    public void Setup(int hP)
    {
        this.hP = hP;
        this.maxHP = hP;
    }
}
