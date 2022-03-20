using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum STATUS
{
    IDLE,
    ATTACK,
    HIT,

}

public partial class SiroCtrl : MonoBehaviour
{
    private EntityInfo entityInfo;
    public SpriteAnimator spriteAnimator;
    public STATUS status = STATUS.IDLE;
    public float timer;

    private void Awake()
    {
        entityInfo = GetComponent<EntityInfo>();
    }

    private void Update()
    {
        switch (status)
        {
            case STATUS.IDLE:
                if (spriteAnimator.CurrentClip != 0)
                    spriteAnimator.Animate(0, true);
                break;

            case STATUS.ATTACK:
                if (spriteAnimator.CurrentClip != 1)
                    spriteAnimator.Animate(1, true);
                break;
        }
    }
}
