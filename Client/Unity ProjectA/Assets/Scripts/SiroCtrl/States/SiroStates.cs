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

        public Attack(SiroCtrl stateMachine)
        {
            siroCtrl = stateMachine;
        }

        public override void Enter()
        {
            siroCtrl.anim.Animate(1, true);
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

            if (siroCtrl.coroutine != null)
                siroCtrl.StopCoroutine(siroCtrl.coroutine);
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
            siroCtrl.fade -= Time.deltaTime;

            if (siroCtrl.fade <= 0f)
            {
                siroCtrl.fade = 0;
            }

            siroCtrl.material.SetFloat("_Fade", siroCtrl.fade);
        }

        public override void Exit()
        {

        }
    }
}
