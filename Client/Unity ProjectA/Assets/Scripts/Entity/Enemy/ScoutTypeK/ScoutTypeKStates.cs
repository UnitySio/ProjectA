using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScoutTypeKStates
{
    public class Create : State
    {
        private ScoutTypeKCtrl owner;

        public Create(ScoutTypeKCtrl stateMachine)
        {
            owner = stateMachine;
        }

        public override void Enter()
        {
            owner.material.color = new Color(191, 2, 0) * Mathf.Pow(2, 5f);
        }

        public override void Update()
        {
            owner.fade += Time.deltaTime;

            if (owner.fade >= 1f)
            {
                owner.fade = 1;
                owner.ChangeState(owner.states[1]);
            }

            owner.material.SetFloat("_Fade", owner.fade);
        }

        public override void Exit()
        {
            owner.material.SetColor("_EmissionColor", Color.white * Mathf.Pow(2, 10f));
            owner.SetHPBar();
        }
    }

    public class Idle : State
    {
        private ScoutTypeKCtrl owner;

        public Idle(ScoutTypeKCtrl stateMachine)
        {
            owner = stateMachine;
        }

        public override void Enter()
        {
            owner.animator.Animate(0, true);
            if (BattleManager.Instance.friendly.Count != 0 && owner.coroutine == null)
                owner.coroutine = owner.StartCoroutine(owner.Attack(owner.states[2]));
        }

        public override void Update()
        {

        }

        public override void Exit()
        {

        }
    }

    public class Attack : State
    {
        private ScoutTypeKCtrl owner;
        private int hitRate;

        public Attack(ScoutTypeKCtrl stateMachine)
        {
            owner = stateMachine;
        }

        public override void Enter()
        {
            owner.animator.Animate(1, true);
            hitRate = Random.Range(0, 101);
        }

        public override void Update()
        {
            if ((owner.animator.currentFrame == 10 || owner.animator.currentFrame == 11 || owner.animator.currentFrame == 12 || owner.animator.currentFrame == 13) && owner.animator.isExecute == false)
            {
                owner.animator.isExecute = true;
                if (BattleManager.Instance.friendly.Count != 0)
                {
                    Entity target = BattleManager.Instance.friendly[0];
                    if (BattleManager.Instance.HitRate(owner, target) > hitRate)
                        owner.StartCoroutine(target.HitTimes(2, BattleManager.Instance.FinalDamage(owner, 1, target)));
                    else
                        owner.StartCoroutine(target.HitTimes(2, 0));
                }
            }

            if (owner.animator.IsPlay == false)
                owner.ChangeState(owner.states[1]);
        }

        public override void Exit()
        {

        }
    }

    public class Hit : State
    {
        private ScoutTypeKCtrl owner;

        public Hit(ScoutTypeKCtrl stateMachine)
        {
            owner = stateMachine;
        }

        public override void Enter()
        {
            owner.animator.Animate(2, true);
        }

        public override void Update()
        {
            if (owner.animator.IsPlay == false)
                owner.ChangeState(owner.states[1]);
        }

        public override void Exit()
        {

        }
    }

    public class Death : State
    {
        private ScoutTypeKCtrl owner;

        public Death(ScoutTypeKCtrl stateMachine)
        {
            owner = stateMachine;
        }

        public override void Enter()
        {
            owner.material.SetColor("_EmissionColor", Color.black);
            owner.animator.Animate(3, true);

            if (owner.coroutine != null)
                owner.StopCoroutine(owner.coroutine);
        }

        public override void Update()
        {
            if (owner.animator.IsPlay == false)
            {
                owner.fade -= Time.deltaTime;

                if (owner.fade <= 0f)
                    owner.fade = 0;

                owner.material.SetFloat("_Fade", owner.fade);
            }
        }

        public override void Exit()
        {

        }
    }
}
