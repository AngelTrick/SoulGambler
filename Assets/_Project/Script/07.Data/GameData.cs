using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
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


    public GameData()
    {
        playerName = "Gambler";
        totalGold = 0;
    }
}
