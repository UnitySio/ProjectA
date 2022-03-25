using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SiroStates;

[RequireComponent(typeof(SpriteAnimator))]
public partial class SiroCtrl : Entity
{
    [HideInInspector]
    public SpriteAnimator anim;

    [HideInInspector]
    public Material material;
    [Range(0f, 1f)]
    public float fade;

    public State[] states = new State[5];

    public Coroutine coroutine;

    private void Awake()
    {
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

        Setup();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override State GetInitState()
    {
        return states[0];
    }

    public override void Hit()
    {
        ChangeState(states[3]);
    }

    public override void Death()
    {
        BattleManager.Instance.friendly.Remove(this);
        ChangeState(states[4]);
    }

    public IEnumerator Attack()
    {
        yield return new WaitForSeconds(actionInterval);
        ChangeState(states[2]);
    }
}
