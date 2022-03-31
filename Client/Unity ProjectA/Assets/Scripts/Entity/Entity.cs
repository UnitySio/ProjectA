using System.Collections;
using UnityEngine;
using System;

public abstract class Entity : StateMachine
{
    public EntityAttribute attribute;

    [HideInInspector]
    public SpriteAnimator anim;

    [HideInInspector]
    public Material material;

    [HideInInspector]
    public float fade;

    private GameObject hPBarGroup;

    public Coroutine coroutine;

    protected virtual void Awake()
    {
        anim = GetComponent<SpriteAnimator>();
        material = GetComponent<SpriteRenderer>().material;
        hPBarGroup = gameObject.transform.GetChild(0).gameObject;
    }

    protected override void Start()
    {
        base.Start();
        hPBarGroup.SetActive(false);
    }

    public void Setup(EntityAttribute attri)
    {
        attribute = attri;
    }

    public virtual void Hurt(int damage)
    {
        attribute.hP -= damage;
        hPBarGroup.GetComponent<HPBar>().HP -= damage;

        if (damage > 0)
            FloatingDamage(damage.ToString());
        else
            FloatingDamage("MISS!");

        if (attribute.hP <= 0)
            Death();
    }

    public abstract void Hit();


    public virtual void Death()
    {
        hPBarGroup.SetActive(false);
        if (gameObject.CompareTag("Friendly"))
            BattleManager.Instance.friendly.Remove(this);
        else if (gameObject.CompareTag("Enemy"))
            BattleManager.Instance.enemy.Remove(this);
    }

    public abstract void PlayHitSFX();

    public abstract void Victory();
    public abstract void Defeat();

    public void SetHPBar()
    {
        hPBarGroup.SetActive(true);
        hPBarGroup.transform.localPosition = SetLocalPosition();
        hPBarGroup.GetComponent<HPBar>().Setup(attribute.hP);
    }

    public void FloatingDamage(string damage)
    {
        GameObject floatingDamage = ObjectPoolManager.Instance.Pop("Floating Damage", this);
        floatingDamage.transform.localPosition = SetLocalPosition();
        floatingDamage.transform.localRotation = Quaternion.identity;
        floatingDamage.GetComponent<FloatingDamage>().Setup(damage);
    }

    public Vector3 SetLocalPosition()
    {
        return new Vector3(0, GetComponent<SpriteRenderer>().bounds.size.y, 0);
    }

    public IEnumerator Attack(State state)
    {
        yield return new WaitForSeconds(attribute.interval);
        ChangeState(state);
    }

    public IEnumerator HitTimes(int hitTimes, int damage)
    {
        Hit();
        PlayHitSFX();

        if (hitTimes == 0 || hitTimes == 1)
            Hurt(damage);
        else
        {
            for (int i = 0; i < hitTimes; i++)
            {
                if (attribute.hP > 0)
                    Hurt(damage);
                else
                    hitTimes = 0;

                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}