using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SiroStates
{
    public class Idle : State
    {
        private SiroCtrl siroCtrl;

        public Idle(SiroCtrl stateMachine)
        {
            siroCtrl = stateMachine;
        }

        public override void Enter()
        {
            if (siroCtrl.anim.CurrentClip != 0)
                siroCtrl.anim.Animate(0, true);
        }

        public override void Update()
        {
            if (siroCtrl.anim.CurrentClip != 0)
                siroCtrl.anim.Animate(0, true);
        }

        public override void Exit()
        {
            if (siroCtrl.anim.CurrentClip != 0)
                siroCtrl.anim.Animate(0, true);
        }
    }
}
