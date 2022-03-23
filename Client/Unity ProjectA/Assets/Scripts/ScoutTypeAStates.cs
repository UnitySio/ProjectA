using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScoutTypeAStates
{
    public class Create : State
    {
        private ScoutTypeACtrl scoutTypeACtrl;

        public Create(ScoutTypeACtrl stateMachine)
        {
            scoutTypeACtrl = stateMachine;
        }

        public override void Enter()
        {
            scoutTypeACtrl.material = scoutTypeACtrl.GetComponent<SpriteRenderer>().material;
        }

        public override void Update()
        {
            scoutTypeACtrl.fade += Time.deltaTime;

            if (scoutTypeACtrl.fade >= 1f)
            {
                scoutTypeACtrl.fade = 1;
                scoutTypeACtrl.ChangeState(scoutTypeACtrl.states[1]);
            }

            scoutTypeACtrl.material.SetFloat("_Fade", scoutTypeACtrl.fade);
        }

        public override void Exit()
        {

        }
    }

    public class Idle : State
    {
        private ScoutTypeACtrl scoutTypeACtrl;

        public Idle(ScoutTypeACtrl stateMachine)
        {
            scoutTypeACtrl = stateMachine;
        }

        public override void Enter()
        {
            scoutTypeACtrl.anim.Animate(0, true);
            scoutTypeACtrl.coroutine = scoutTypeACtrl.StartCoroutine(scoutTypeACtrl.Attack());
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
        private ScoutTypeACtrl scoutTypeACtrl;

        public Attack(ScoutTypeACtrl stateMachine)
        {
            scoutTypeACtrl = stateMachine;
        }

        public override void Enter()
        {
            scoutTypeACtrl.anim.Animate(1, true);
        }

        public override void Update()
        {
            if (scoutTypeACtrl.anim.IsPlay == false)
                scoutTypeACtrl.ChangeState(scoutTypeACtrl.states[1]);
        }

        public override void Exit()
        {

        }
    }

    public class Death : State
    {
        private ScoutTypeACtrl scoutTypeACtrl;

        public Death(ScoutTypeACtrl stateMachine)
        {
            scoutTypeACtrl = stateMachine;
        }

        public override void Enter()
        {
            scoutTypeACtrl.anim.Animate(2, true);

            if (scoutTypeACtrl.coroutine != null)
                scoutTypeACtrl.StopCoroutine(scoutTypeACtrl.coroutine);
        }

        public override void Update()
        {
            scoutTypeACtrl.fade -= Time.deltaTime;

            if (scoutTypeACtrl.fade <= 0f)
            {
                scoutTypeACtrl.fade = 0;
            }

            scoutTypeACtrl.material.SetFloat("_Fade", scoutTypeACtrl.fade);
        }

        public override void Exit()
        {

        }
    }
}
