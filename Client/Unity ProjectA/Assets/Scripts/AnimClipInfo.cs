using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimClipInfo
{
    public string name;
    public bool isLoop;
    public bool isNextClip;
    public int nextClip;
    public List<AnimClip> animationClip = new List<AnimClip>();
}
