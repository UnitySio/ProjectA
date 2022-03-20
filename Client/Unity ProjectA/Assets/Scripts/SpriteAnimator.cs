using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private bool play;
    public bool Play
    {
        get { return play; }
        set
        {
            play = value;
            currentFrame = 0;

            if (coroutine != null)
                StopCoroutine(coroutine);

            if (value == true)
                coroutine = StartCoroutine(isPlay());
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
            loop = animationClips[CurrentClip].loop;
        }
    }

    public int currentFrame;
    public List<AnimClip> animationClips = new List<AnimClip>();

    private Coroutine coroutine = null;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private IEnumerator isPlay()
    {
        while (Play)
        {
            spriteRenderer.sprite = animationClips[CurrentClip].animationClip[currentFrame];
            yield return new WaitForSeconds(1 / frameRate);

            currentFrame = (currentFrame + 1) % animationClips[CurrentClip].animationClip.Count;

            if (!loop && currentFrame == animationClips[CurrentClip].animationClip.Count - 1)
                Play = false;
        }
    }

    public void Animate(int clipNumber, bool play)
    {
        CurrentClip = clipNumber;
        Play = play;
    }
}
