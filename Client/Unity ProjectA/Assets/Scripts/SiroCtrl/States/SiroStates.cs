using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SiroStates
{
    public class Create : State
    {
        private SiroCtrl siroCtrl;

        public Create(SiroCtrl stateMachine)
        {
            siroCtrl = stateMachine;
        }

        public override void Enter()
        {
            siroCtrl.material = siroCtrl.GetComponent<SpriteRenderer>().material;
        }

        public override void Update()
        {
            siroCtrl.fade += Time.deltaTime;

            if (siroCtrl.fade >= 1f)
            {
                siroCtrl.fade = 1;
                siroCtrl.ChangeState(siroCtrl.states[1]);
            }

            siroCtrl.material.SetFloat("_Fade", siroCtrl.fade);
        }

        public override void Exit()
        {

        }
    }

    public class Idle : State
    {
        private SiroCtrl siroCtrl;

        public Idle(SiroCtrl stateMachine)
        {
            siroCtrl = stateMachine;
        }

        public override void Enter()
        {
            siroCtrl.anim.Animate(0, true);
            if (BattleManager.Instance.enemy.Count != 0)
                siroCtrl.coroutine = siroCtrl.StartCoroutine(siroCtrl.Attack());
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
        private SiroCtrl siroCtrl;
        private bool isDamageExecuted;

        public Attack(SiroCtrl stateMachine)
        {
            siroCtrl = stateMachine;
        }

        public override void Enter()
        {
            isDamageExecuted = false;
            siroCtrl.anim.Animate(1, true);
        }

        public override void Update()
        {
            if (siroCtrl.anim.currentFrame == 4 && isDamageExecuted == false)
            {
                isDamageExecuted = true;
                if (BattleManager.Instance.enemy.Count != 0)
                {
                    Entity _target = BattleManager.Instance.enemy[0];
                    _target.Hurt(BattleManager.Instance.FinalDamage(siroCtrl.attack, 1, _target.defense, siroCtrl.attackCorrection, siroCtrl.level, _target.level));
                }
            }

            if (siroCtrl.anim.IsPlay == false)
                siroCtrl.ChangeState(siroCtrl.states[1]);
        }

        public override void Exit()
        {
            isDamageExecuted = false;
        }
    }

    public class Hit : State
    {
        private SiroCtrl siroCtrl;

        public Hit(SiroCtrl stateMachine)
        {
            siroCtrl = stateMachine;
        }

        public override void Enter()
        {
            siroCtrl.anim.Animate(2, true);
        }

        public override void Update()
        {
            if (siroCtrl.anim.IsPlay == false)
                siroCtrl.ChangeState(siroCtrl.states[1]);
        }

        public override void Exit()
        {

        }
    }

    public class Death : State
    {
        private SiroCtrl siroCtrl;

        public Death(SiroCtrl stateMachine)
        {
            siroCtrl = stateMachine;
        }

        public override void Enter()
        {
            siroCtrl.anim.Animate(3, true);

            if (siroCtrl.coroutine != null)
                siroCtrl.StopCoroutine(siroCtrl.coroutine);
        }

        public override void Update()
        {
            if (siroCtrl.anim.IsPlay == false)
            {
                siroCtrl.fade -= Time.deltaTime;

                if (siroCtrl.fade <= 0f)
                {
                    siroCtrl.fade = 0;
                }

                siroCtrl.material.SetFloat("_Fade", siroCtrl.fade);
            }
        }

        public override void Exit()
        {

        }
    }
}
