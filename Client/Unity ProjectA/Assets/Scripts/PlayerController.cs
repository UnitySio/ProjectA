using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody rigidBody;
    public SpriteRenderer SpriteRenderer;
    public SpriteAnimator spriteAnimator;

    public int moveSpeed;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        spriteAnimator = GetComponent<SpriteAnimator>();
    }

    private void Start()
    {
        spriteAnimator.Animate(0, true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            rigidBody.AddForce(new Vector3(0, 5, 0), ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (h != 0 || v != 0)
        {
            if (spriteAnimator.CurrentClip != 1)
                spriteAnimator.Animate(1, true);
            
            transform.Translate(h * moveSpeed * Time.deltaTime, 0, v * moveSpeed * Time.deltaTime);
        }
        
        if (h == 0 && v == 0)
            if (spriteAnimator.CurrentClip != 0)
                spriteAnimator.Animate(0, true);

        if (h > 0)
            SpriteRenderer.flipX = false;
        
        if (h < 0)
            SpriteRenderer.flipX = true;
    }
}
