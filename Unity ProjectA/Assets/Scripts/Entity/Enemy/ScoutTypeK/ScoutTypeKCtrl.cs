using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScoutTypeKStates;

[RequireComponent(typeof(SpriteAnimator))]
public class ScoutTypeKCtrl : Entity
{
    public State[] states = new State[5];

    protected override void Awake()
    {
        base.Awake();
        states[0] = new Create(this);
        states[1] = new Idle(this);
        states[2] = new Attack(this);
        states[3] = new Hit(this);
        states[4] = new Death(this);

        attribute.uid = 0;
        attribute.name = "Scout Type K";
        attribute.lv = 25;
        attribute.hp = 999999999;
        attribute.attack = 6140000;
        attribute.defense = 100;
        attribute.dodge = 20;
        attribute.hit = 15;
        attribute.interval = 0.2f;
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
