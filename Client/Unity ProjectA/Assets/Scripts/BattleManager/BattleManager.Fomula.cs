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
        float result = level - defenderLevel > 0 ? 1 + Mathf.Pow(Mathf.Clamp(level - defenderLevel, -levelDifference, levelDifference), 2) * levelCorrection :
            1 + Mathf.Pow(Mathf.Clamp(level - defenderLevel, -levelDifference, levelDifference), 2) * levelCorrection;
        return result;
    }

    // ���� ������(������ ���ݷ�, ������ ���ݷ� ����, ����� ����, ������ ���ݷ� ����, ������ ����, ����� ����)
    public int FinalDamage(int attack, int attackRate, int defense, int attackCorrection, int level, int defenderLevel)
    {
        float result = (Damage(attack, attackRate, defense) + DamageCorrection(attack, attackRate, attackCorrection)) * LevelWeight(level, defenderLevel);
        return (int)Mathf.Round(result);
    }

    // ���߷�(����� ȸ��, ������ ����)
    public float HitRate(int dodge, int hit)
    {
        float result = ((float)hitDifference / (hitDifference + Mathf.Clamp(dodge - hit, 0, dodge - hit))) * 100;
        return result;
    }
}
