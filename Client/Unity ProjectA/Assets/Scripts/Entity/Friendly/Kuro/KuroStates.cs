using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KuroStates
{
    public class Create : State
    {
        private KuroCtrl owner;

        public Create(KuroCtrl stateMachine)
        {
            owner = stateMachine;
        }

        public override void Enter()
        {
            owner.material.color = new Color(0, 23, 191) * Mathf.Pow(2, 5f);
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
            owner.SetHPBar();
        }
    }

    public class Idle : State
    {
        private KuroCtrl owner;

        public Idle(KuroCtrl stateMachine)
        {
            owner = stateMachine;
        }

        public override void Enter()
        {
            owner.animator.Animate(0, true);
            if (BattleManager.Instance.enemy.Count != 0 && owner.coroutine == null)
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
        private KuroCtrl owner;
        private int hitRate;

        public Attack(KuroCtrl stateMachine)
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
            if (owner.animator.currentFrame == 4 && owner.animator.isExecute == false)
            {
                owner.animator.isExecute = true;
                if (BattleManager.Instance.enemy.Count != 0)
                {
                    Entity target = BattleManager.Instance.enemy[0];
                    if (BattleManager.Instance.HitRate(owner, target) > hitRate)
                        owner.StartCoroutine(target.HitTimes(10, BattleManager.Instance.FinalDamage(owner, 1, target)));
                    else
                        owner.StartCoroutine(target.HitTimes(10, 0));
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
        private KuroCtrl owner;

        public Hit(KuroCtrl stateMachine)
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
        private KuroCtrl owner;

        public Death(KuroCtrl stateMachine)
        {
            owner = stateMachine;
        }

        public override void Enter()
        {
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

    public class Victory : State
    {
        private KuroCtrl owner;

        public Victory(KuroCtrl stateMachine)
        {
            owner = stateMachine;
        }

        public override void Enter()
        {
            owner.animator.Animate(4, true);

            if (owner.coroutine != null)
                owner.StopCoroutine(owner.coroutine);
        }

        public override void Update()
        {

        }

        public override void Exit()
        {

        }
    }
}
