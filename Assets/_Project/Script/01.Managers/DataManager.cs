using System.IO;
using System.Text;
using UnityEngine;
public class DataManager : MonoBehaviour
{
    private static DataManager _instance;
    public static DataManager Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<DataManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("DataManger");
                    _instance = go.AddComponent<DataManager>();
                }
            }
            return _instance;
        }
    }

    public GameData currentGameData;
    private string _savePath;
    public int currentStageGold = 0;
    //캡슐화된 프로퍼티 (외부 읽기 전용)
    public int TotalGold
    {
        get => currentGameData != null ? currentGameData.totalGold : 0;
    }
    private readonly string _encryptionKey = "SoulGamblerSecretKey";
    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _savePath = Path.Combine(Application.persistentDataPath, "savefile.json");
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void AddGold(int amount)
    {
        if(currentGameData != null)
        {
            currentGameData.totalGold += amount;
            SaveGame();
        }
    }
    public void AddStageGold(int amount)
    {
        currentStageGold += amount;
    }
    
    public void SaveGame()
    {
        if (currentGameData == null) currentGameData = new GameData();
        currentGameData.totalGold += currentStageGold;
        currentStageGold = 0;
        string json = JsonUtility.ToJson(currentGameData,true);
        string encryptedJson = EncryptDecrypt(json);
        File.WriteAllText(_savePath, encryptedJson);

        Debug.Log($"게임 저장 완료 (암호화됨) {_savePath}");
    
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
            string encryptedJson = File.ReadAllText(_savePath);
            string json = EncryptDecrypt(encryptedJson);
            currentGameData = JsonUtility.FromJson<GameData>(json);
            Debug.Log($"게임 로드 완료 : 골드 : {currentGameData.totalGold}");
        }
        catch (System.Exception e)
        {
            Debug.Log($"세이브 파일 로드 중 오류 발생 : {e.Message}\n 데이터를 초기화 합니다.");
            currentGameData = new GameData();
        }
    }


    private string EncryptDecrypt(string data)
    {
        StringBuilder modifiedData = new StringBuilder();
        for(int i =0; i < data.Length; i++)
        {
            modifiedData.Append((char)(data[i] ^ _encryptionKey[i % _encryptionKey.Length]));
        }
        return modifiedData.ToString();
    }
    public void ResetData()
    {
        currentGameData = new GameData();
        SaveGame();
        Debug.Log("데이터 초기화 완료");
    }
}

