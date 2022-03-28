using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KuroStates;
using System.Threading;

[RequireComponent(typeof(SpriteAnimator))]
public partial class KuroCtrl : Entity
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

        // Setup(0, "Kuro", 30, 999999999, 245600, 5, 100, 35, 25, 1f);
        attribute.no = 0;
        attribute.name = "Kuro";
        attribute.level = 30;
        attribute.hP = 999999999;
        attribute.attack = 245600;
        attribute.defense = 100;
        attribute.dodge = 35;
        attribute.hit = 25;
        attribute.interval = 1f;
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
        if (damage > 0)
            if (currentState != states[3])
                ChangeState(states[3]);
    }

    public override void Death()
    {
        base.Death();
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
}
