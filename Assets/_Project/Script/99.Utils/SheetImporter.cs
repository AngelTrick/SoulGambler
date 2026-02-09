// ==========================================
// FILE NAME: SheetImporter.cs
// 설명: 구글 시트 CSV 파서 (안전 장치 및 CSV 포맷 자동 매칭 적용)
// ==========================================

using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class SheetImporter : EditorWindow
{
    private string _charUrl = "";
    private string _weaponUrl = "";
    private string _enemyUrl = "";
    private string _rewardUrl = "";

    private const string KEY_CHAR = "Sheet_Char_URL";
    private const string KEY_WEAPON = "Sheet_Weapon_URL";
    private const string KEY_ENEMY = "Sheet_Enemy_URL";
    private const string KEY_REWARD = "Sheet_Reward_URL";


    [MenuItem("Tools/Data Importer")]
    public static void ShowWindow()
    {
        GetWindow<SheetImporter>("Data Importer");
    }

    private void OnEnable()
    {
        _charUrl = EditorPrefs.GetString(KEY_CHAR, "");
        _weaponUrl = EditorPrefs.GetString(KEY_WEAPON, "");
        _enemyUrl = EditorPrefs.GetString(KEY_ENEMY, "");
        _rewardUrl = EditorPrefs.GetString(KEY_REWARD, "");
    }

    private void OnDisable()
    {
        EditorPrefs.SetString(KEY_CHAR, _charUrl);
        EditorPrefs.SetString(KEY_WEAPON, _weaponUrl);
        EditorPrefs.SetString(KEY_ENEMY, _enemyUrl);
        EditorPrefs.SetString(KEY_REWARD, _rewardUrl);
    }

    void OnGUI()
    {
        GUILayout.Label("Google Sheet Live Sync Tool (Final Ver)", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        GUILayout.Label("CSV Publish URLs (File > Share > Publish to web > CSV)", EditorStyles.miniLabel);
        EditorGUILayout.Space();
        _charUrl = EditorGUILayout.TextField("Character URL", _charUrl);
        _weaponUrl = EditorGUILayout.TextField("Weapon URL", _weaponUrl);
        _enemyUrl = EditorGUILayout.TextField("Enemy URL", _enemyUrl);
        _rewardUrl = EditorGUILayout.TextField("Reward URL", _rewardUrl);

        EditorGUILayout.Space();

        if (GUILayout.Button("Sync All Data (Download & Update)", GUILayout.Height(40)))
        {
            SyncData();
        }
    }
    async void SyncData()
    {
        //1. Character
        if (!string.IsNullOrEmpty(_charUrl))
        {
            string data = await DownloadCSV(_charUrl);
            if (data != null) ImportCharacters(data);
        }
        //2. Weapon
        if (!string.IsNullOrEmpty(_weaponUrl))
        {
            string data = await DownloadCSV(_weaponUrl);
            if (data != null) ImportWeapons(data);
        }
        //3. Enemy
        if (!string.IsNullOrEmpty(_enemyUrl))
        {
            string data = await DownloadCSV(_enemyUrl);
            if (data != null) ImportEnemies(data);
        }
        //4. Reward
        if (!string.IsNullOrEmpty(_rewardUrl))
        {
            string data = await DownloadCSV(_rewardUrl);
            if (data != null) ImportRewards(data);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"<color=cyan>모든 데이터 동기화 및 갱신 완료!</color>");
    }
    async Task<string> DownloadCSV(string url)
    {
        using (UnityWebRequest www  = UnityWebRequest.Get(url))
        {
            var operation = www.SendWebRequest();

            while (!operation.isDone) await Task.Yield();

            if(www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"다운로드 실패 ({url}) : {www.error}");
                return null;
            }

            return www.downloadHandler.text;
        }
    }


    // ---------------------------------------------------------
    // [1] 캐릭터 데이터 파싱 (10개 열)
    // ---------------------------------------------------------
    void ImportCharacters(string csvData)
    {
        string savePath = "Assets/_Project/SO/Characters";
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

        string[] lines = csvData.Replace("\r\n", "\n").Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            string[] row = line.Split(',');

            // 최소 ID, Name은 있어야 함
            if (row.Length < 2) continue;

            string id = ParseString(row, 0);
            string name = ParseString(row, 1);
            string fileName = $"Char_{id}_{name}.asset";

            PlayerDataSO data = GetOrCreateSO<PlayerDataSO>(savePath, fileName);

            data.jobName = name;
            data.maxHP = ParseFloat(row, 2);
            data.defense = ParseFloat(row, 3);
            data.moveSpeed = ParseFloat(row, 4);
            data.damage = ParseFloat(row, 5);
            data.critChance = ParseFloat(row, 6);
            data.critDamage = ParseFloat(row, 7);
            data.magnet = ParseFloat(row, 8);
            data.luck = ParseFloat(row, 9);
        }
    }

    // ---------------------------------------------------------
    // [2] 무기 데이터 파싱 (12개 열 + 옵션)
    // ---------------------------------------------------------
    void ImportWeapons(string csvData)
    {
        string savePath = "Assets/_Project/SO/Weapon";
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

        string[] lines = csvData.Replace("\r\n", "\n").Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            string[] row = line.Split(',');

            if (row.Length < 2) continue;

            string id = ParseString(row, 0);
            string name = ParseString(row, 1);
            string fileName = $"Weapon_{id}_{name}.asset";

            WeaponDataSO data = GetOrCreateSO<WeaponDataSO>(savePath, fileName);

            data.weaponName = name;

            string typeStr = ParseString(row, 2).ToLower();
            data.weaponType = (typeStr == "melee") ? WeaponType.Melee : WeaponType.Ranged;

            string gradeStr = ParseString(row, 3);
            if (System.Enum.TryParse(gradeStr, true, out ItemGrade gradeResult)) data.grade = gradeResult;
            else data.grade = ItemGrade.Normal;

            data.baseDamage = ParseFloat(row, 4);
            data.attackSpeed = ParseFloat(row, 5);
            data.range = ParseFloat(row, 6);
            data.projectileSpeed = ParseFloat(row, 7);
            data.amount = ParseInt(row, 8, 1); // 기본값 1
            data.pierce = ParseInt(row, 9);
            data.knockback = ParseFloat(row, 10);
            data.description = ParseString(row, 11);

            // [옵션] 프리팹 연결 (13번째 열이 있다면)
            string prefabName = ParseString(row, 12);
            if (!string.IsNullOrEmpty(prefabName))
            {
                GameObject prefab = FindPrefabByName(prefabName);
                if (prefab != null) data.projectilePrefab = prefab;
            }
        }
    }

    // ---------------------------------------------------------
    // [3] 적 데이터 파싱 (12개 열)
    // ---------------------------------------------------------
    void ImportEnemies(string csvData)
    {
        string savePath = "Assets/_Project/SO/Enemy";
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

        string[] lines = csvData.Replace("\r\n", "\n").Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            string[] row = line.Split(',');

            if (row.Length < 2) continue;

            string id = ParseString(row, 0);
            string name = ParseString(row, 1);
            string fileName = $"Enemy_{id}_{name}.asset";

            EnemyDataSO data = GetOrCreateSO<EnemyDataSO>(savePath, fileName);

            data.enemyName = name;

            string rankStr = ParseString(row, 2);
            if (System.Enum.TryParse(rankStr, true, out EnemyRank rankResult)) data.rank = rankResult;
            else data.rank = EnemyRank.Normal;

            string raceStr = ParseString(row, 3);
            if (System.Enum.TryParse(raceStr, true, out EnemyRace raceResult)) data.race = raceResult;
            else data.race = EnemyRace.Undead;

            data.maxHP = ParseFloat(row, 4);
            data.defense = ParseFloat(row, 5);
            data.moveSpeed = ParseFloat(row, 6);
            data.damage = ParseFloat(row, 7);
            data.detectRange = ParseFloat(row, 8);
            data.attackRange = ParseFloat(row, 9);
            data.expReward = ParseInt(row, 10);
            data.goldReward = ParseInt(row, 11);
        }
    }
    // ---------------------------------------------------------
    // [4] 레벨업 보상 파싱 
    // ---------------------------------------------------------

    void ImportRewards(string csvData)
    {
        string savePath = "Assets/_Project/SO/Rewards";
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

        string[] lines = csvData.Replace("\r\n", "\n").Split('\n');

        for(int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            string[] row = line.Split(',');
            if(row.Length < 2) continue;

            string id = ParseString(row, 0);
            string title = ParseString(row, 1);
            string safeTitle =title.Replace(" ", "_");
            string fileName = $"Reward_{id}.asset";

            LevelUpRewardSO data = GetOrCreateSO<LevelUpRewardSO>(savePath, fileName);

            data.title = title;
            data.description = ParseString(row,2);
            data.value = ParseFloat(row, 4);

            string typeStr = ParseString(row, 3);
            if (System.Enum.TryParse(typeStr, true, out WeaponUpgradeType wType))
            {
                data.weaponType = wType;
                data.statType = StatType.MaxHP;
            }
            else if(System.Enum.TryParse(typeStr, true , out StatType sType))
            {
                data.statType = sType;
                data.weaponType = WeaponUpgradeType.None;
            }
            else
            {
                Debug.LogWarning($"[Reward] 알 수 없는 타입입니다. {typeStr} (ID : {id}");
            }

        }
        Debug.Log("보상(Reward) 데이터 갱신 완료 (무기 / 스탯 자동 분류");
    }
    // ---------------------------------------------------------
    // [헬퍼 함수] 안전한 파싱 (데이터가 없거나 에러나면 기본값 리턴)
    // ---------------------------------------------------------

    // 문자열 읽기 (없으면 빈 문자열)
    string ParseString(string[] row, int index, string defaultValue = "")
    {
        if (index >= row.Length) return defaultValue;
        return row[index].Trim();
    }

    // 실수 읽기 (없으면 0)
    float ParseFloat(string[] row, int index, float defaultValue = 0f)
    {
        if (index >= row.Length) return defaultValue;
        if (float.TryParse(row[index].Trim(), out float result)) return result;
        return defaultValue;
    }

    // 정수 읽기 (없으면 0)
    int ParseInt(string[] row, int index, int defaultValue = 0)
    {
        if (index >= row.Length) return defaultValue;
        if (int.TryParse(row[index].Trim(), out int result)) return result;
        return defaultValue;
    }

    // SO 파일 생성 또는 로드 (연결 유지)
    T GetOrCreateSO<T>(string savePath, string fileName) where T : ScriptableObject
    {
        // 파일명에 사용할 수 없는 문자 제거
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }

        string fullPath = $"{savePath}/{fileName}";
        T data = AssetDatabase.LoadAssetAtPath<T>(fullPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(data, fullPath);
        }
        EditorUtility.SetDirty(data);
        return data;
    }

    // 이름으로 프리팹 찾기
    GameObject FindPrefabByName(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName)) return null;
        string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
        return null;
    }
}