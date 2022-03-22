using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SCOUT_TYPE_A_STATUS
{
    IDLE,
    ATTACK,
    DEATH
}

public class ScoutTypeACtrl : MonoBehaviour
{
    private UnitInfo unitInfo;
    private SpriteAnimator spriteAnimator;
    public SCOUT_TYPE_A_STATUS status = SCOUT_TYPE_A_STATUS.IDLE;

    private void Awake()
    {
        unitInfo = GetComponent<UnitInfo>();
        spriteAnimator = GetComponent<SpriteAnimator>();
    }

    private void Start()
    {
        spriteAnimator.Animate(0, true);
        StartCoroutine(Attack());
    }

    private void Update()
    {
        if (unitInfo.HP == 0)
            if (status != SCOUT_TYPE_A_STATUS.DEATH)
                status = SCOUT_TYPE_A_STATUS.DEATH;

        switch (status)
        {
            case SCOUT_TYPE_A_STATUS.IDLE:
                if (spriteAnimator.CurrentClip != 0)
                {
                    spriteAnimator.Animate(0, true);
                    StartCoroutine(Attack());
                }
                break;

            case SCOUT_TYPE_A_STATUS.ATTACK:
                if (spriteAnimator.CurrentClip != 1)
                    spriteAnimator.Animate(1, true);

                if (spriteAnimator.CurrentClip == 1 && spriteAnimator.IsPlay == false)
                    status = SCOUT_TYPE_A_STATUS.IDLE;
                break;

            case SCOUT_TYPE_A_STATUS.DEATH:
                if (spriteAnimator.CurrentClip != 2)
                    spriteAnimator.Animate(2, true);

                if (spriteAnimator.CurrentClip == 2 && spriteAnimator.IsPlay == false)
                    Destroy(gameObject);
                break;

            default:
                break;
        }
    }

    IEnumerator Attack()
    {
        yield return new WaitForSeconds(unitInfo.actionInterval);
        status = SCOUT_TYPE_A_STATUS.ATTACK;
        BattleManager.Instance.friendly[0].HP -= BattleManager.Instance.FinalDamage(unitInfo.attack, 1, BattleManager.Instance.friendly[0].defense, unitInfo.attackCorrection, unitInfo.level, BattleManager.Instance.friendly[0].level);
    }
}
