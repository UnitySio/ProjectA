using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BattleManager : MonoBehaviour
{
    [Header("BattleManager.Fomula")]
    [SerializeField]
    private int standardDefense = 100;
    [SerializeField]
    private float levelCorrection = 0.008f;
    [SerializeField]
    private int levelDifference = 10;
    [SerializeField]
    private int hitDifference = 20;

    /// <summary>
    /// 방어력
    /// </summary>
    /// <param name="defense">방어자의 방어력</param>
    /// <returns>float</returns>
    public float Defense(int defense)
    {
        float result = (float)standardDefense / (defense + standardDefense);
        return result;
    }

    /// <summary>
    /// 데미지
    /// </summary>
    /// <param name="attack">공격자의 공격력</param>
    /// <param name="attackRate">공격자의 공격력 배율</param>
    /// <param name="defense">방어자의 방어력</param>
    /// <returns>float</returns>
    public float Damage(int attack, int attackRate, int defense)
    {
        float result = (attack * attackRate) * Defense(defense);
        return result;
    }

    /// <summary>
    /// 데미지 보정
    /// </summary>
    /// <param name="attack">공격자의 공격력</param>
    /// <param name="attackRate">공격자의 공격력 배율</param>
    /// <param name="attackCorrection">공격자의 공격력 보정값</param>
    /// <returns>float</returns>
    public float DamageCorrection(int attack, int attackRate, int attackCorrection)
    {
        float result = (attack * attackRate) * ((float)attackCorrection / 100);
        return result;
    }

    /// <summary>
    /// 레벨 편차 가중치
    /// </summary>
    /// <param name="level">공격자의 레벨</param>
    /// <param name="defenderLevel">방어자의 레벨</param>
    /// <returns>float</returns>
    public float LevelWeight(int level, int defenderLevel)
    {
        float result = 0;
        if (level - defenderLevel > 0)
            result = 1 + Mathf.Pow(Mathf.Clamp(level - defenderLevel, level - defenderLevel, levelDifference), 2) * levelCorrection;
        else
            result = 1 - Mathf.Pow(Mathf.Clamp(level - defenderLevel, -levelDifference, level - defenderLevel), 2) * levelCorrection;
        return result;
    }

    /// <summary>
    /// 최종 데미지
    /// </summary>
    /// <param name="attacker">공격자의 Entity 스크립트</param>
    /// <param name="attackRate">공격자의 공격력 배율</param>
    /// <param name="defender">방어자의 Entity 스크립트</param>
    /// <returns>int</returns>
    public int FinalDamage(Entity attacker, int attackRate, Entity defender)
    {
        float result = (Damage(attacker.attribute.attack, attackRate, defender.attribute.defense) + DamageCorrection(attacker.attribute.attack, attackRate, attacker.attribute.attackCorrection)) * LevelWeight(attacker.attribute.level, defender.attribute.level);
        return (int)Mathf.Round(result);
    }

    /// <summary>
    /// 적중 확률
    /// </summary>
    /// <param name="attacker">공격자의 Entity</param>
    /// <param name="defender">방어자의 Entity</param>
    /// <returns>int</returns>
    public int HitRate(Entity attacker, Entity defender)
    {
        float result = ((float)hitDifference / (hitDifference + Mathf.Clamp(defender.attribute.dodge - attacker.attribute.hit, 0, defender.attribute.dodge - attacker.attribute.hit))) * 100;
        return (int)result;
    }
}
