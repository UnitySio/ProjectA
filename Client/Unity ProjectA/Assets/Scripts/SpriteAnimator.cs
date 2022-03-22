using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private Coroutine coroutine;

    [SerializeField]
    private bool isPlay;
    public bool IsPlay
    {
        get { return isPlay; }
        set
        {
            isPlay = value;
            currentFrame = 0;

            if (coroutine != null)
                StopCoroutine(coroutine);

            if (value == true)
                coroutine = StartCoroutine(Play());
        }
    }

    private bool loop;
    public float frameRate;

    [SerializeField]
    private int currentClip;
    public int CurrentClip
    {
        get { return currentClip; }
        set
        {
            currentClip = value;
            loop = animationClips[CurrentClip].isLoop;
        }
    }

    public int currentFrame;
    public List<AnimClip> animationClips = new List<AnimClip>();

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private IEnumerator Play()
    {
        while (IsPlay)
        {
            spriteRenderer.sprite = animationClips[CurrentClip].animationClip[currentFrame];

            if (!loop && currentFrame == animationClips[CurrentClip].animationClip.Count - 1)
            {
                isPlay = false;
                if (animationClips[CurrentClip].isNextClip) 
                    Animate(animationClips[CurrentClip].nextClip, true);
            }

            yield return new WaitForSeconds(1 / frameRate);

            currentFrame = (currentFrame + 1) % animationClips[CurrentClip].animationClip.Count;
        }
    }

    public void Animate(int clipNumber, bool isPlay)
    {
        CurrentClip = clipNumber;
        IsPlay = isPlay;
    }
}
