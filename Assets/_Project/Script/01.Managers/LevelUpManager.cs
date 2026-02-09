using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance;

    [Header("Settings")]
    public int optionCount = 3;

    public List<LevelUpRewardSO> rewardPool;

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
            if(rewardPool != null && rewardPool.Count > 0)
            {
                int randomIndex = Random.Range(0, rewardPool.Count);
                LevelUpRewardSO selectedData = rewardPool[randomIndex];

                option.title = selectedData.title;
                option.description = selectedData.description;
                option.icon = selectedData.icon != null ? selectedData.icon : (currentWeapon != null ? currentWeapon.icon : null);
                if(selectedData.weaponType != WeaponUpgradeType.None)
                {
                    option.type = RewardType.UpgradeWeapon;
                    option.weaponUpgradeType = selectedData.weaponType;
                    option.statValue = selectedData.value;

                    option.weaponData = currentWeapon;
                }
                else
                {
                    option.type = RewardType.StatUp;
                    option.statType = selectedData.statType;
                    option.statValue = selectedData.value;
                }
            }
            else
            { 
                option.type = RewardType.StatUp;
                option.title = "보상 데이터 없음";
                option.description = "Inspector에 SO를 등록해주세요";
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
