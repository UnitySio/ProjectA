using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScoutTypeKStates;

[RequireComponent(typeof(SpriteAnimator))]
public class ScoutTypeKCtrl : Entity
{
    public State[] states = new State[4];

    protected override void Awake()
    {
        base.Awake();
        states[0] = new Create(this);
        states[1] = new Idle(this);
        states[2] = new Attack(this);
        states[3] = new Death(this);

        attribute.no = 0;
        attribute.name = "Scout Type K";
        attribute.level = 25;
        attribute.hP = 999999999;
        attribute.attack = 6140000;
        attribute.defense = 100;
        attribute.dodge = 20;
        attribute.hit = 15;
        attribute.interval = 0.2f;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override State GetInitiateState()
    {
        return states[0];
    }

    public override void Hit(int damage)
    {
        // ChangeState(states[3]);
    }

    public override void Death()
    {
        base.Death();
        ChangeState(states[3]);
    }

    public override void Victory()
    {

    }

    public override void Defeat()
    {

    }
}
