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

        Setup(0, "Scout Type A", 25, 1000000000, 30, 5, 100, 20, 15, 4);
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
        base.Hit(damage);
        // ChangeState(states[3]);
    }

    public override void Death()
    {
        BattleManager.Instance.enemy.Remove(this);
        ChangeState(states[3]);
    }

    public override void Victory()
    {

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
