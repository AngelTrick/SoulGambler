using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum EnemyRank { Normal, Epic, Boss}

public enum EnemyRace { Undead, Human, Beast, Construct, Demon}

[CreateAssetMenu(fileName = "EnemyData", menuName = "EnemySO/EnemyData")]
public class EnemyDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public string enemyName;
    public EnemyRank rank;
    public EnemyRace race;

    [Header("전투 스탯")]
    public float maxHP;
    public float defense;
    public float moveSpeed;
    public float damage;
    public float detectRange;
    public float attackRange;

    [Header("보상")]
    public int expReward;
    public int goldReward;
}
