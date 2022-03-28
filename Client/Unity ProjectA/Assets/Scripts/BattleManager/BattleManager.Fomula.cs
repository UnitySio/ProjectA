using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BattleManager : MonoBehaviour
{
    [Header("BattleManager.Fomula")]
    public int standardDefense = 100;
    public float levelCorrection = 0.008f;
    public int levelDifference = 10;
    public int hitDifference = 20;

    // ����(����� ����)
    public float Defense(int defense)
    {
        float result = (float)standardDefense / (defense + standardDefense);
        return result;
    }

    // ������(������ ���ݷ�, ������ ���ݷ� ����, ����� ����)
    public float Damage(int attack, int attackRate, int defense)
    {
        float result = (attack * attackRate) * Defense(defense);
        return result;
    }

    // ������ ����(������ ���ݷ�, ������ ���ݷ� ����, ������ ���ݷ� ����)
    public float DamageCorrection(int attack, int attackRate, int attackCorrection)
    {
        float result = (attack * attackRate) * ((float)attackCorrection / 100);
        return result;
    }

    // ���� ���� ����ġ(������ ����, ����� ����)
    public float LevelWeight(int level, int defenderLevel)
    {
        float result = 0;
        if (level - defenderLevel > 0)
            result = 1 + Mathf.Pow(Mathf.Clamp(level - defenderLevel, level - defenderLevel, levelDifference), 2) * levelCorrection;
        else
            result = 1 - Mathf.Pow(Mathf.Clamp(level - defenderLevel, -levelDifference, level - defenderLevel), 2) * levelCorrection;
        return result;
    }

    // ���� ������(������ ���ݷ�, ������ ���ݷ� ����, ����� ����, ������ ���ݷ� ����, ������ ����, ����� ����)
    public int FinalDamage(Entity attacker, int attackRate, Entity defender)
    {
        float result = (Damage(attacker.attribute.attack, attackRate, defender.attribute.defense) + DamageCorrection(attacker.attribute.attack, attackRate, attacker.attribute.attackCorrection)) * LevelWeight(attacker.attribute.level, defender.attribute.level);
        return (int)Mathf.Round(result);
    }

    // ���߷�(����� ȸ��, ������ ����)
    public int HitRate(Entity attacker, Entity defender)
    {
        float result = ((float)hitDifference / (hitDifference + Mathf.Clamp(defender.attribute.dodge - attacker.attribute.hit, 0, defender.attribute.dodge - attacker.attribute.hit))) * 100;
        return (int)result;
    }
}
