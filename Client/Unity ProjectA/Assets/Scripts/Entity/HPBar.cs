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

    public GameObject hPBar;
    public GameObject hPBarMask;

    private void Awake()
    {
        origin = hPBarMask.transform.localScale.x;
    }

    private void Start()
    {
        if (gameObject.transform.parent.CompareTag("Friendly"))
            hPBar.GetComponent<SpriteRenderer>().color = Color.green;
        else if (gameObject.transform.parent.CompareTag("Enemy"))
            hPBar.GetComponent<SpriteRenderer>().color = Color.red;
    }

    private void Update()
    {
        offset = (hP / maxHP) * origin;
        hPBarMask.transform.localScale = new Vector3(offset, hPBarMask.transform.localScale.y, hPBarMask.transform.localScale.z);
    }

    public void Setup(int hP)
    {
        this.hP = hP;
        this.maxHP = hP;
    }
}
