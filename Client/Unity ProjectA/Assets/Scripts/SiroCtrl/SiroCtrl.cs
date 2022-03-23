using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SiroStates;

public partial class SiroCtrl : StateMachine
{
    [HideInInspector]
    public UnitInfo unit;
    [HideInInspector]
    public SpriteAnimator anim;

    public Material material;
    [Range(0f, 1f)]
    public float fade;

    public State[] states = new State[5];

    public Coroutine coroutine;

    public int hp;

    private void Awake()
    {
        unit = GetComponent<UnitInfo>();
        anim = GetComponent<SpriteAnimator>();

        states[0] = new Create(this);
        states[1] = new Idle(this);
        states[2] = new Attack(this);
        states[3] = new Hit(this);
        states[4] = new Death(this);
    }

    protected override void Start()
    {
        base.Start();

        hp = unit.HP;
    }

    protected override void Update()
    {
        base.Update();

        if (unit.HP < hp && currentState != states[3])
        {
            hp = unit.HP;
            ChangeState(states[3]);
        }

        if (unit.HP == 0 && currentState != states[4])
        {
            ChangeState(states[4]);
        }
    }

    protected override State GetInitState()
    {
        return states[0];
    }

    public IEnumerator Attack()
    {
        yield return new WaitForSeconds(unit.actionInterval);
        ChangeState(states[2]);
    }
}
