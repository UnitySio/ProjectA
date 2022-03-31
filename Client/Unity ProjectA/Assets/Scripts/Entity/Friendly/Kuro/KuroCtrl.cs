using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KuroStates;
using System.Threading;

[RequireComponent(typeof(SpriteAnimator))]
public partial class KuroCtrl : Entity
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

        attribute.uID = 0;
        attribute.name = "Kuro";
        attribute.level = 30;
        attribute.hP = 999999999;
        attribute.attack = 6140000;
        attribute.attackCorrection = 1;
        attribute.defense = 100;
        attribute.dodge = 35;
        attribute.hit = 25;
        attribute.interval = 2f;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        if (BattleManager.Instance.isVictory && currentState != states[5])
            Victory();
    }

    protected override State GetInitiateState()
    {
        return states[0];
    }

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

    public override void PlayHitSFX()
    {

    }

    public override void Victory()
    {
        ChangeState(states[5]);
    }

    public override void Defeat()
    {

    }
}
