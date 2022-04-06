using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance
    {
        get
        {
            if (instance == null) return null;
            return instance;
        }
    }

    public int sFXChannel;
    public AudioSource[] sFXChannels;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
            Destroy(instance);
    }

    public void PlaySFX(AudioClip audioClip)
    {
        if (sFXChannels[sFXChannel].isPlaying)
            sFXChannel = (sFXChannel + 1) % sFXChannels.Length;

        sFXChannels[sFXChannel].clip = audioClip;
        sFXChannels[sFXChannel].Play();
    }
}
