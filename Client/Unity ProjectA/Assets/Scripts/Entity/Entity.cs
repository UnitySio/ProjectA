using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public List<SFX> sFXClips = new List<SFX>();
    public Dictionary<string, AudioClip> sFXClipDictionary = new Dictionary<string, AudioClip>();

    protected virtual void Awake()
    {
        anim = GetComponent<SpriteAnimator>();
        material = GetComponent<SpriteRenderer>().material;
        hPBarGroup = gameObject.transform.GetChild(0).gameObject;

        foreach (SFX sFX in sFXClips)
            sFXClipDictionary.Add(sFX.key, sFX.audioClip);
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
        coroutine = null;
    }

    public IEnumerator HitTimes(int hitTimes, int damage)
    {
        Hit();
        SoundManager.Instance.PlaySFX(sFXClipDictionary["Hit"]);

        if (hitTimes == 0 || hitTimes == 1)
            Hurt(damage);
        else
        {
            for (int i = 0; i < hitTimes; i++)
            {
                if (attribute.hP > 0)
                    Hurt(damage);
                else
                    yield break;

                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}