using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : StateMachine
{
    public EntityAttribute attribute;

    [HideInInspector]
    public SpriteAnimator animator;

    [HideInInspector]
    public Material material;

    [HideInInspector]
    public float fade;

    private GameObject hpBarGroup;

    public Coroutine coroutine;

    public List<SFX> sfxClips = new List<SFX>();
    public Dictionary<string, AudioClip> sfxClipDictionary = new Dictionary<string, AudioClip>();

    protected virtual void Awake()
    {
        animator = GetComponent<SpriteAnimator>();
        material = GetComponent<SpriteRenderer>().material;
        hpBarGroup = gameObject.transform.GetChild(0).gameObject;

        foreach (SFX sfx in sfxClips)
            sfxClipDictionary.Add(sfx.key, sfx.audioClip);
    }

    protected override void Start()
    {
        base.Start();
        hpBarGroup.SetActive(false);
    }

    public void Setup(EntityAttribute attribute)
    {
        this.attribute = attribute;
    }

    public void Hurt(int damage)
    {
        attribute.hp -= damage;
        hpBarGroup.GetComponent<HPBar>().HP -= damage;

        if (damage > 0)
            FloatingDamage(damage.ToString());
        else
            FloatingDamage("MISS!");

        if (attribute.hp <= 0)
            Death();
    }

    public abstract void Hit();


    public virtual void Death()
    {
        hpBarGroup.SetActive(false);
        if (gameObject.CompareTag("Friendly"))
            BattleManager.Instance.friendly.Remove(this);
        else if (gameObject.CompareTag("Enemy"))
            BattleManager.Instance.enemy.Remove(this);
    }

    public void SetHPBar()
    {
        hpBarGroup.SetActive(true);
        hpBarGroup.transform.localPosition = SetLocalPosition();
        hpBarGroup.GetComponent<HPBar>().Setup(attribute.hp);
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

    public void PlaySFX(string key)
    {
        SoundManager.Instance.PlaySFX(sfxClipDictionary[key]);
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
        PlaySFX("Hit");

        if (hitTimes == 0 || hitTimes == 1)
            Hurt(damage);
        else
        {
            for (int i = 0; i < hitTimes; i++)
            {
                if (attribute.hp > 0)
                    Hurt(damage);
                else
                    yield break;

                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}