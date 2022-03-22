using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SiroStates;

public partial class SiroCtrl : StateMachine
{
    public UnitInfo unit;
    public SpriteAnimator anim;

    public State[] states = new State[1];

    private void Awake()
    {
        unit = GetComponent<UnitInfo>();
        anim = GetComponent<SpriteAnimator>();

        states[0] = new Idle(this);
    }

    protected override State GetInitState()
    {
        return states[0];
    }
}
