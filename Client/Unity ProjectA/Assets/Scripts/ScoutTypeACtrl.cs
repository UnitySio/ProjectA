using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TYPE_A_STATUS
{
    IDLE,
    ATTACK,
    DEATH
}

public class ScoutTypeACtrl : MonoBehaviour
{
    private UnitInfo unitInfo;
    private SpriteAnimator spriteAnimator;
    public TYPE_A_STATUS status = TYPE_A_STATUS.IDLE;

    private void Awake()
    {
        unitInfo = GetComponent<UnitInfo>();
        spriteAnimator = GetComponent<SpriteAnimator>();
    }

    private void Start()
    {
        spriteAnimator.Animate(0, true);
    }
}
