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
    public List<SFX> sFXClips = new List<SFX>();
    public Dictionary<string, AudioClip> sFXClipsDictionary = new Dictionary<string, AudioClip>();

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

    private void Start()
    {
        foreach (SFX sfx in sFXClips)
            sFXClipsDictionary.Add(sfx.key, sfx.audioClip);
    }

    public void SFXPlay(string key)
    {
        if (sFXChannels[sFXChannel].isPlaying)
            sFXChannel = (sFXChannel + 1) % sFXChannels.Length;

        sFXChannels[sFXChannel].clip = sFXClipsDictionary[key];
        sFXChannels[sFXChannel].Play();
    }
}
