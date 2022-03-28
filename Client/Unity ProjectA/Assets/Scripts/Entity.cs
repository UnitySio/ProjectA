using System.Collections;
using UnityEngine;
using System;

public abstract class Entity : StateMachine
{
    public GameObject hPBarGroup;
    public EntityAttribute attribute;

    public void Setup(EntityAttribute attri)
    {
        attribute = attri;
    }

    public virtual void Hurt(int damage)
    {
        attribute.hP -= damage;
        hPBarGroup.GetComponent<HPBar>().HP -= damage;

        if (attribute.hP > 0)
            Hit(damage);
        else
            Death();
    }

    public virtual void Hit(int damage)
    {
        if (damage > 0)
            FloatingDamage(damage.ToString());
        else
            FloatingDamage("MISS!");
    }


    public virtual void Death()
    {
        hPBarGroup.SetActive(false);
    }

    public abstract void Victory();
    public abstract void Defeat();

    public void SetHPBar()
    {
        hPBarGroup.SetActive(true);
        hPBarGroup.transform.localPosition = SetLocalPosition(hPBarGroup.transform);
        hPBarGroup.GetComponent<HPBar>().Setup(attribute.hP);
    }

    public void FloatingDamage(string damage)
    {
        GameObject floatingDamage = Instantiate(BattleManager.Instance.floatingDamage, transform.position, transform.rotation, transform);
        floatingDamage.transform.localPosition = SetLocalPosition(floatingDamage.transform);
        floatingDamage.GetComponent<FloatingDamage>().Setup(damage);
    }

    public Vector3 SetLocalPosition(Transform pos)
    {
        return new Vector3(pos.localPosition.x, pos.localPosition.y + GetComponent<SpriteRenderer>().bounds.size.y, pos.localPosition.z);
    }

    public IEnumerator Attack(State state)
    {
        yield return new WaitForSeconds(attribute.interval);
        ChangeState(state);
    }

    public IEnumerator HitTimes(int hitTimes, int damage)
    {
        if (hitTimes == 0 || hitTimes == 1)
            Hurt(damage);
        else
        {
            for (int i = 0; i < hitTimes; i++)
            {
                if (attribute.hP > 0)
                    Hurt(damage);
                else hitTimes = 0;

                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}