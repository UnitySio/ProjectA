using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    private Entity owner;
    private SpriteRenderer spriteRenderer;

    private Coroutine coroutine;

    public bool isExecute;

    [SerializeField] private bool isPlay;

    public bool IsPlay
    {
        get => isPlay;
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

    [SerializeField] private int currentClip;

    public int CurrentClip
    {
        get => currentClip;
        set
        {
            currentClip = value;
            if (currentFrame != 0) currentFrame = 0;
            loop = animationClips[CurrentClip].isLoop;
        }
    }

    public int currentFrame;
    public List<AnimClipInfo> animationClips = new List<AnimClipInfo>();

    private void Awake()
    {
        owner = GetComponent<Entity>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (isPlay) Animate(currentClip, true);
    }

    private IEnumerator Play()
    {
        while (IsPlay)
        {
            spriteRenderer.sprite = animationClips[CurrentClip].animationClip[currentFrame].sprite;

            if (animationClips[CurrentClip].animationClip[currentFrame].sFXKey != "")
                SoundManager.Instance.PlaySFX(
                    owner.sfxClipDictionary[animationClips[CurrentClip].animationClip[currentFrame].sFXKey]);

            if (!loop && currentFrame == animationClips[CurrentClip].animationClip.Count - 1)
            {
                isPlay = false;
                isExecute = false;
            }

            yield return new WaitForSeconds(1 / frameRate);

            currentFrame = (currentFrame + 1) % animationClips[CurrentClip].animationClip.Count;

            isExecute = false;
        }
    }

    public void Animate(int clipNumber, bool isPlay)
    {
        CurrentClip = clipNumber;
        IsPlay = isPlay;
    }
}
