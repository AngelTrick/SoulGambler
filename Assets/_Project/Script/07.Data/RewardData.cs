using UnityEngine;

public enum RewardType
{
    NewWeapon,
    UpgradeWeapon,
    StatUp,
    Consumable
}
[System.Serializable]
public class RewardOption
{
    public RewardType type;
    public string title;
    public string description;
    public Sprite icon;

    public StatType statType;
    public float statValue;
    public WeaponDataSO weaponData;

    public WeaponUpgradeType weaponUpgradeType;

}
