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

    public State[] states = new State[6];

    public Coroutine coroutine;

    private void Awake()
    {
        anim = GetComponent<SpriteAnimator>();

        states[0] = new Create(this);
        states[1] = new Idle(this);
        states[2] = new Attack(this);
        states[3] = new Hit(this);
        states[4] = new Death(this);
        states[5] = new Victory(this);

        Setup(0, "Siro", 30, 1000, 283749000, 5, 100, 20, 15, 0.2f);
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

    public override void Hit(int damage)
    {
        base.Hit(damage);
        ChangeState(states[3]);
    }

    public override void Death()
    {
        BattleManager.Instance.friendly.Remove(this);
        ChangeState(states[4]);
    }

    public override void Victory()
    {
        ChangeState(states[5]);
    }

    public override void Defeat()
    {

    }

    public IEnumerator Attack()
    {
        yield return new WaitForSeconds(interval);
        ChangeState(states[2]);
    }
}
