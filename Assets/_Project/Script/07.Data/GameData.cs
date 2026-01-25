using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GameData 
{
    public string playerName = "New Player";
    public int totalGold = 0;

    public int maxKillCount = 0;
    public float bestSurvivalTime = 0f;

    public List<string> unlockWeaponIds = new List<string>();

    public int attackDamageLevel = 0;
    public int moveSpeedLevel = 0;

    public GameData()
    {
        playerName = "Gambler";
        totalGold = 0;
        unlockWeaponIds = new List<string>() { "NoviceWand" };
    }
}
