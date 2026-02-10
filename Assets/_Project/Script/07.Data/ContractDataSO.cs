using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum ContractType
{
    StatTrade,
    WeaponGrant,
    GoldGrant
}
public enum StatType
{
    None,
    MaxHP,
    Damage,
    MoveSpeed,
    Defense,
    CritChance
}
[CreateAssetMenu(fileName = "NewContract", menuName = "SO/ContractData")]
public class ContractDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string contractName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Logic")]
    public ContractType type;

    [Header("Cost")]
    public StatType costStat;
    public float costValue;

    [Header("Reward")]
    public StatType rewardStat;
    public float rewardValue;

    public WeaponDataSO rewardWeapon;
    public int rewardGold;

}
