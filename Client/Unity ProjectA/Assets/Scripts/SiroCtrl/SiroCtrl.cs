using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SIRO_STATUS
{
    IDLE,
    ATTACK,
    HIT,

}

public partial class SiroCtrl : MonoBehaviour
{
    private UnitInfo unitInfo;
    private SpriteAnimator spriteAnimator;
    public SIRO_STATUS status = SIRO_STATUS.IDLE;
    public float timer;

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
        switch (status)
        {
            case SIRO_STATUS.IDLE:
                if (spriteAnimator.CurrentClip != 0)
                {
                    spriteAnimator.Animate(0, true);
                    StartCoroutine(Attack());
                }
                break;

            case SIRO_STATUS.ATTACK:
                if (spriteAnimator.CurrentClip != 1)
                    spriteAnimator.Animate(1, true);

                if (spriteAnimator.CurrentClip == 1 && spriteAnimator.IsPlay == false)
                    status = SIRO_STATUS.IDLE;
                break;
        }
    }

    IEnumerator Attack()
    {
        yield return new WaitForSeconds(unitInfo.actionInterval);
        status = SIRO_STATUS.ATTACK;
        //BattleManager.Instance.enemy[0].hp -= BattleManager.Instance.FinalDamage(unitInfo.attack, 1, BattleManager.Instance.enemy[0].defense, unitInfo.attackCorrection, unitInfo.hp, BattleManager.Instance.enemy[0].defense);
    }
}
