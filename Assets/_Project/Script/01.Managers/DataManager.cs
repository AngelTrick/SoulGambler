using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using JetBrains.Annotations;
using UnityEngine.UIElements;
using UnityEngine.Rendering.Universal;
public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    public GameData currentGameData;

    private string _savePath;

    public int currentStageGold = 0;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            _savePath = Path.Combine(Application.persistentDataPath, "savefile.json");
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void AddStageGold(int amount)
    {
        currentStageGold += amount;
    }
    
    public void SaveGame()
    {
        currentGameData.totalGold += currentStageGold;
        currentStageGold = 0;

        string json = JsonUtility.ToJson(currentGameData,true);
        File.WriteAllText(_savePath, json);

        Debug.Log($"게임 저장 완료 : 경로 {_savePath}\n {json}");
    
    }
    public void LoadGame()
    {
        if(!File.Exists( _savePath))
        {
            Debug.Log("현재 저장 된 파일이 없어 새로운 데이터 생성합니다.");
            currentGameData = new GameData();
            SaveGame();
            return;
        }
        try
        {
            string json = File.ReadAllText(_savePath);
            currentGameData = JsonUtility.FromJson<GameData>(json);
            Debug.Log($"게임 로드 완료 : 골드 : {currentGameData.totalGold}");
        }
        catch (System.Exception e)
        {
            Debug.Log($"세이브 파일 로드 중 오류 발생 : {e.Message}\n 데이터를 초기화 합니다.");
            currentGameData = new GameData();
        }
    }

    public void ResetData()
    {
        currentGameData = new GameData();
        SaveGame();
        Debug.Log("데이터 초기화 완료");
    }
}

