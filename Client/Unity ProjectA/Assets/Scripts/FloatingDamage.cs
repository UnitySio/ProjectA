using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingDamage : Poolable
{
    private TextMeshPro text;
    public float moveSpeed;
    public float alphaSpeed;
    private Color alpha;
    public float destroyTime;
    
    private void Awake()
    {
        text = GetComponent<TextMeshPro>();
        alpha = text.color;
    }

    private void Update()
    {
        transform.Translate(new Vector3(0, moveSpeed * Time.deltaTime, 0));
        alpha.a = Mathf.Lerp(alpha.a, 0, alphaSpeed * Time.deltaTime);
        text.color = alpha;
    }

    private void OnEnable()
    {
        Invoke("Destroy", destroyTime);
    }

    private void OnDisable()
    {
        alpha.a = 255;
    }

    public void Setup(string damage)
    {
        text.text = damage;
    }

    public void Destroy()
    {
        Push();
    }
}
