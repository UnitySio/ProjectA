using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UGUIAnimator : MonoBehaviour
{
    private Image image;
    public Sprite[] sprites;
    public bool isPlay;
    public float frameRate;
    public int currentFrame;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Start()
    {
        if (isPlay) StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        while (true)
        {
            image.sprite = sprites[currentFrame];
            
            if (currentFrame == sprites.Length - 1)
                isPlay = false;
            
            yield return new WaitForSeconds(1 / frameRate);
            currentFrame = (currentFrame + 1) % sprites.Length;
        }
    }
}
