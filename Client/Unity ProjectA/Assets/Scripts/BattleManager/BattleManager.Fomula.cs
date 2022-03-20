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

    // 방어력(방어자 방어력)
    public float Defense(int defense)
    {
        float result = (float)standardDefense / (defense + standardDefense);
        return result;
    }

    // 데미지(공격자 공격력, 공격자 공격력 배율, 방어자 방어력)
    public float Damage(int attack, int attackRate, int defense)
    {
        float result = (attack * attackRate) * Defense(defense);
        return result;
    }

    // 데미지 보정(공격자 공격력, 공격자 공격력 배율, 공격자 공격력 보정)
    public float DamageCorrection(int attack, int attackRate, int attackCorrection)
    {
        float result = (attack * attackRate) * ((float)attackCorrection / 100);
        return result;
    }

    // 레벨 편차 가중치(공격자 레벨, 방어자 레벨)
    public float LevelWeight(int level, int defenderLevel)
    {
        float result = level - defenderLevel > 0 ? 1 + Mathf.Pow(Mathf.Clamp(level - defenderLevel, -levelDifference, levelDifference), 2) * levelCorrection :
            1 + Mathf.Pow(Mathf.Clamp(level - defenderLevel, -levelDifference, levelDifference), 2) * levelCorrection;
        return result;
    }

    // 최좋 데미지(공격자 공격력, 공격자 공격력 배율, 방어자 방어력, 공격자 공격력 보정, 공격자 레벨, 방어자 레벨)
    public int FinalDamage(int attack, int attackRate, int defense, int attackCorrection, int level, int defenderLevel)
    {
        float result = (Damage(attack, attackRate, defense) + DamageCorrection(attack, attackRate, attackCorrection)) * LevelWeight(level, defenderLevel);
        return (int)Mathf.Round(result);
    }

    // 적중률(방어자 회피, 공격자 적중)
    public float HitRate(int dodge, int hit)
    {
        float result = ((float)hitDifference / (hitDifference + Mathf.Clamp(dodge - hit, 0, dodge - hit))) * 100;
        return result;
    }
}
