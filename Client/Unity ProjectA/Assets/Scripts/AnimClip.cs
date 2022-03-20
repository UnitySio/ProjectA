using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimClip
{
    public string name;
    public bool loop;
    public List<Sprite> animationClip = new List<Sprite>();
}
