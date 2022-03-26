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
            scoutTypeACtrl.CreateHPBar();
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
            if (BattleManager.Instance.friendly.Count != 0)
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
        private bool isDamageExecuted;

        public Attack(ScoutTypeACtrl stateMachine)
        {
            scoutTypeACtrl = stateMachine;
        }

        public override void Enter()
        {
            isDamageExecuted = false;
            scoutTypeACtrl.anim.Animate(1, true);
        }

        public override void Update()
        {
            if (scoutTypeACtrl.anim.currentFrame == 4 && isDamageExecuted == false)
            {
                isDamageExecuted = true;
                if (BattleManager.Instance.friendly.Count != 0)
                {
                    Entity _target = BattleManager.Instance.friendly[0];
                    _target.Hurt(BattleManager.Instance.FinalDamage(scoutTypeACtrl.attack, 1, _target.defense, scoutTypeACtrl.attackCorrection, scoutTypeACtrl.level, _target.level));

                }
            }

            if (scoutTypeACtrl.anim.IsPlay == false)
                scoutTypeACtrl.ChangeState(scoutTypeACtrl.states[1]);
        }

        public override void Exit()
        {
            isDamageExecuted = false;
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
            if (scoutTypeACtrl.anim.IsPlay == false)
            {
                scoutTypeACtrl.fade -= Time.deltaTime;

                if (scoutTypeACtrl.fade <= 0f)
                {
                    scoutTypeACtrl.fade = 0;
                }

                scoutTypeACtrl.material.SetFloat("_Fade", scoutTypeACtrl.fade);
            }
        }

        public override void Exit()
        {

        }
    }
}
