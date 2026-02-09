using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewStatReward", menuName = "SO/LevelUpReward")]
public class LevelUpRewardSO : ScriptableObject
{
    [Header("UI")]
    public string title;
    [TextArea] public string description;
    public Sprite icon;

    [Header("보상 타입 (둘 중 하나만 설정")]
    public StatType statType;
    public WeaponUpgradeType weaponType;

    [Header("적용 수치")]
    public float value;
}
