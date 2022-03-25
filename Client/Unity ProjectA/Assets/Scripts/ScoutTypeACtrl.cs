using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScoutTypeAStates;

[RequireComponent(typeof(SpriteAnimator))]
public class ScoutTypeACtrl : Entity
{
    [HideInInspector]
    public SpriteAnimator anim;

    [HideInInspector]
    public Material material;
    [Range(0f, 1f)]
    public float fade;

    public State[] states = new State[4];

    public Coroutine coroutine;

    private void Awake()
    {
        anim = GetComponent<SpriteAnimator>();

        states[0] = new Create(this);
        states[1] = new Idle(this);
        states[2] = new Attack(this);
        states[3] = new Death(this);
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

    }

    public override void Death()
    {
        BattleManager.Instance.enemy.Remove(this);
        ChangeState(states[3]);
    }

    public IEnumerator Attack()
    {
        yield return new WaitForSeconds(actionInterval);
        ChangeState(states[2]);
    }
}
