using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
[System.Serializable]
public class StatBonusData
{
    public StatType statType;
    public float bonusValue;

    public StatBonusData(StatType type, float value)
    {
        this.statType = type;
        this.bonusValue = value;
    }
}

[System.Serializable]
public class GameData
{
    public string playerName = "New Player";
    public int totalGold = 0;
    public int maxKillCount = 0;
    public float bestSurvivalTime = 0f;

    public List<string> unlockWeaponIds = new List<string>() {"IronSword"};
    public List<string> unlockCharacterIds = new List<string>() {"Knight"};

    public int attackLevel = 0;
    public int healthLevel = 0;
    public int speedLevel = 0;
    public int defenseLevel = 0;
    public int critChanceLevel = 0;
    public int magnetLevel = 0;

    public List<StatBonusData> contractBonusList = new List<StatBonusData>();

    public GameData()
    {
        playerName = "Gambler";
        totalGold = 0;
    }
    public float GetContractBonusValue(StatType type)
    {
        foreach(var data in contractBonusList)
        {
            if(data.statType == type)
            {
                return data.bonusValue;
            }
        }
        return 0f;
    }
    public void AddContractBonusValue(StatType type , float amount)
    {
        foreach(var data in contractBonusList)
        {
            if(data.statType == type)
            {
                data.bonusValue += amount;
                return;
            }
        }
        contractBonusList.Add(new StatBonusData(type, amount));
    }
}
