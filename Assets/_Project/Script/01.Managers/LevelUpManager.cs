using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance;

    [Header("Settings")]
    public int optionCount = 3;

    public List<RewardOption> currentRewards = new List<RewardOption>();

    private void Awake()
    {
        Instance = this;
    }

    public List<RewardOption> GetRandomRewards()
    {
        currentRewards.Clear();

        WeaponDataSO currentWeapon = null;
        if(PlayerController.Instance != null)
        {
            var weaponSys = PlayerController.Instance.GetComponent<WeaponSystem>();
            if (weaponSys != null) currentWeapon = weaponSys.currentWeapon;
        }
            for (int i = 0; i < optionCount; i++)
            {
                RewardOption option = new RewardOption();

                if (Random.value > 0.5f && currentWeapon != null)
                {
                    option.type = RewardType.UpgradeWeapon;
                    option.title = $"{currentWeapon.weaponName}강화";
                    option.description = "공격력 + 2 / 쿨타임 -0.1초";
                    option.icon = currentWeapon.icon;
                    option.weaponData = currentWeapon;
                }
                else
                {
                    option.type = RewardType.StatUp;
                    int randomStat = Random.Range(0, 5);

                    switch (randomStat)
                    {
                        case 0:
                            option.title = "최대 체력 증가";
                            option.description = "최대 체력이 10 증가합니다.";
                            option.statType = StatType.MaxHP;
                            option.statValue = 10f;
                            break;
                        case 1:
                            option.title = "기본 공격력 증가";
                            option.description = "공격력이 2 증가합니다.";
                            option.statType = StatType.Damage;
                            option.statValue = 2f;
                            break;
                        case 2:
                            option.title = "이동 속도 증가";
                            option.description = "이동 속도 5% 빨라집니다.";
                            option.statType = StatType.MoveSpeed;
                            option.statValue = 0.05f;
                            break;
                        case 3:
                            option.title = "방어력 증가";
                            option.description = "받는 피해이 1 감소합니다.";
                            option.statType = StatType.Defense;
                            option.statValue = 1f;
                            break;
                        case 4:
                            option.title = "치명타 확률 증가";
                            option.description = "치명타 확률 5% 증가합니다.";
                            option.statType = StatType.CritChance;
                            option.statValue = 5f;
                            break;
                        default:
                            option.title = "알수 없는 보상";
                            option.description = "";
                            option.statType = StatType.MaxHP;
                            option.statValue = 0f;
                            break;
                    }
                }
            currentRewards.Add(option);
            }
            return currentRewards;
    }
    public void SelectRewardByIndex(int index)
    {
        if (index < 0 || index >= currentRewards.Count) return;
        SelectReward(currentRewards[index]);
    }

    public void SelectReward(RewardOption reward)
    {
        if(PlayerController.Instance != null)
        {
            PlayerController.Instance.ApplyReward(reward);
        }
        if(UIManager.Instance != null)
        {
            UIManager.Instance.CloseLevelUpUI();
        }
    }
}
