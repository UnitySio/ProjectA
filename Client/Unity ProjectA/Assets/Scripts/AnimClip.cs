using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimClip
{
    public string name;
    public bool isLoop;
    public bool isNextClip;
    public int nextClip;
    public List<Sprite> animationClip = new List<Sprite>();
}
