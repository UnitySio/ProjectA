using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KuroStates;
using System.Threading;

[RequireComponent(typeof(SpriteAnimator))]
public class KuroCtrl : Entity
{
    public State[] states = new State[6];

    protected override void Awake()
    {
        base.Awake();
        states[0] = new Create(this);
        states[1] = new Idle(this);
        states[2] = new Attack(this);
        states[3] = new Hit(this);
        states[4] = new Death(this);
        states[5] = new Victory(this);

        attribute.uid = 0;
        attribute.name = "Kuro";
        attribute.lv = 30;
        attribute.hp = 999999999;
        attribute.attack = 6140000;
        attribute.defense = 100;
        attribute.dodge = 35;
        attribute.hit = 25;
        attribute.interval = 2f;
    }

    protected override State GetInitiateState() => states[0];

    public override void Hit()
    {
        if (currentState != states[2])
            if (currentState != states[3])
                ChangeState(states[3]);
    }

    public override void Death()
    {
        base.Death();
        ChangeState(states[4]);
    }
}
