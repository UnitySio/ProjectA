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

    public int sfxChannel;
    public AudioSource[] sfxChannels;

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
        if (sfxChannels[sfxChannel].isPlaying)
            sfxChannel = (sfxChannel + 1) % sfxChannels.Length;

        sfxChannels[sfxChannel].clip = audioClip;
        sfxChannels[sfxChannel].Play();
    }
}
