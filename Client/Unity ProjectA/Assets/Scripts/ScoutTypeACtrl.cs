using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScoutTypeAStates;

public class ScoutTypeACtrl : StateMachine
{
    [HideInInspector]
    public UnitInfo unit;
    [HideInInspector]
    public SpriteAnimator anim;

    public Material material;
    [Range(0f, 1f)]
    public float fade;

    public State[] states = new State[4];

    public Coroutine coroutine;

    public int hp;

    private void Awake()
    {
        unit = GetComponent<UnitInfo>();
        anim = GetComponent<SpriteAnimator>();

        states[0] = new Create(this);
        states[1] = new Idle(this);
        states[2] = new Attack(this);
        states[3] = new Death(this);
    }

    protected override void Start()
    {
        base.Start();

        hp = unit.HP;
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.A)) unit.HP -= BattleManager.Instance.FinalDamage(2406, 1, 100, 5, 30, 25);

        /*if (unit.HP < hp && currentState != states[3])
        {
            hp = unit.HP;
            ChangeState(states[3]);
        }*/

        if (unit.HP == 0 && currentState != states[3])
        {
            ChangeState(states[3]);
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
        BattleManager.Instance.friendly[0].HP -= BattleManager.Instance.FinalDamage(unit.attack, 1, BattleManager.Instance.friendly[0].defense, unit.attackCorrection, unit.level, BattleManager.Instance.friendly[0].level);
    }
}
