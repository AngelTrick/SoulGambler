// ==========================================
// FILE NAME: SheetImporter.cs
// 설명: 구글 시트 데이터 연동 툴 (Final Fixed Ver)
// 기능: One-Key Sync, 스마트 업데이트, 자동 연결, 특수문자/따옴표 제거 파싱
// ==========================================

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class SheetImporter : EditorWindow
{
    private string _spreadsheetId = "";

    private const string SHEET_NAME_CHAR = "Characters";
    private const string SHEET_NAME_WEAPON = "Weapons";
    private const string SHEET_NAME_ENEMY = "Enemies";
    private const string SHEET_NAME_REWARD = "Rewards";

    private const string KEY_ID = "Sheet_Spreadsheet_ID";

    private HashSet<string> _processedAssets = new HashSet<string>();

    [MenuItem("Tools/Google Sheet Importer(One-Key)")]
    public static void ShowWindow()
    {
        GetWindow<SheetImporter>("Sheet Importer");
    }

    private void OnEnable()
    {
        _spreadsheetId = EditorPrefs.GetString(KEY_ID,"");
    }

    private void OnDisable()
    {
        EditorPrefs.SetString(KEY_ID, _spreadsheetId);
    }

    void OnGUI()
    {
        GUILayout.Label("Google Sheet One-Key Sync Tool (Final Ver)", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        GUILayout.Label("1. 구글 시트 우측 상단 [공유] -> [링크가 있는 모든 사용자] 설정", EditorStyles.helpBox);
        GUILayout.Label("2. 브라우저 주소창의 '/d/' 와 '/edit' 사이의 ID만 복사하세요.", EditorStyles.miniLabel);
        GUILayout.Label("3. 기존 파일 업데이트 (중복 생성 방지)", EditorStyles.miniLabel);
        GUILayout.Label("4. LevelUpManager 자동 연결 기능 포함", EditorStyles.miniLabel);
        EditorGUILayout.Space();

        _spreadsheetId = EditorGUILayout.TextField("Spreadsheet ID", _spreadsheetId);

        EditorGUILayout.Space();

        if (GUILayout.Button("Sync All Data", GUILayout.Height(40)))
        {
            if (string.IsNullOrEmpty(_spreadsheetId))
            {
                Debug.LogError("Spreadsheet ID를 입력해주세요");
                return;
            }
            SyncData();
        }
    }
    async void SyncData()
    {
        Debug.Log("데이터 동기화 시작");
        _processedAssets.Clear();
        //1. Character
        string charData = await DownloadCSV(GetDownloadUrl(SHEET_NAME_CHAR));
        if (charData != null) ImportCharacters(charData);
        //2. Weapon
        string weaponData = await DownloadCSV(GetDownloadUrl(SHEET_NAME_WEAPON));
        if (weaponData != null) ImportWeapons(weaponData);
        //3. Enemy
        string enemyData = await DownloadCSV(GetDownloadUrl(SHEET_NAME_ENEMY));
        if (enemyData != null) ImportEnemies(enemyData);
        //4. Reward
        string rewardData = await DownloadCSV(GetDownloadUrl(SHEET_NAME_REWARD));
        if (rewardData != null)
        {
            ImportRewards(rewardData);
            AutoLinkRewardsToManager();
        }
        CleanupMissingAssets();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"<color=cyan>모든 데이터 동기화 및 갱신 완료!</color>");
    }
    string GetDownloadUrl(string sheetName)
    {
        return $"https://docs.google.com/spreadsheets/d/{_spreadsheetId}/gviz/tq?tqx=out:csv&sheet={sheetName}";
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
            
            string fileName = $"Char_{SanitizeFileName(id)}_{SanitizeFileName(name)}.asset";

            PlayerDataSO data = FindAndRenameSO<PlayerDataSO>(savePath, id, fileName, "Char_");

            data.jobName = name;
            data.maxHP = ParseFloat(row, 2);
            data.defense = ParseFloat(row, 3);
            data.moveSpeed = ParseFloat(row, 4);
            data.damage = ParseFloat(row, 5);
            data.critChance = ParseFloat(row, 6);
            data.critDamage = ParseFloat(row, 7);
            data.magnet = ParseFloat(row, 8);
            data.luck = ParseFloat(row, 9);

            EditorUtility.SetDirty(data);

            RegisterAssetPath(savePath, fileName);
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
            
            string fileName = $"Weapon_{SanitizeFileName(id)}_{SanitizeFileName(name)}.asset";

            WeaponDataSO data = FindAndRenameSO<WeaponDataSO>(savePath, id, fileName, "Weapon_");

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

            EditorUtility.SetDirty(data);

            RegisterAssetPath(savePath, fileName);
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
           
            string fileName = $"Enemy_{SanitizeFileName(id)}_{SanitizeFileName(name)}.asset";

            EnemyDataSO data = FindAndRenameSO<EnemyDataSO>(savePath, id, fileName, "Enemy_");

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

            EditorUtility.SetDirty(data);

            RegisterAssetPath(savePath, fileName);
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
            string safeTitle =SanitizeFileName(title.Replace(" ", "_"));
            string fileName = $"Reward_{SanitizeFileName(id)}_{safeTitle}.asset";

            LevelUpRewardSO data = FindAndRenameSO<LevelUpRewardSO>(savePath, id, fileName, "Reward_");

            data.title = title;
            data.description = ParseString(row,2);
            data.value = ParseFloat(row, 4);

            string typeStr = ParseString(row, 3);
            if (System.Enum.TryParse(typeStr, true, out WeaponUpgradeType wType))
            {
                data.weaponType = wType;
                data.statType = StatType.None;
            }
            else if(System.Enum.TryParse(typeStr, true , out StatType sType))
            {
                data.statType = sType;
                data.weaponType = WeaponUpgradeType.None;
            }
            else 
            {
                Debug.LogWarning($"[Reward] 알수 없는 타입: {typeStr}(ID :{id}");
            }
            EditorUtility.SetDirty(data);

            RegisterAssetPath(savePath, fileName);

        }
        Debug.Log("보상(Reward) 데이터 갱신 완료 (무기 / 스탯 자동 분류)");
    }
    void RegisterAssetPath(string folder, string fileName)
    {
        string fullPath = $"{folder}/{fileName}";
        _processedAssets.Add(fullPath);
    }
    void CleanupMissingAssets()
    {
        string[] targetFolders = {
            "Assets/_Project/SO/Characters",
            "Assets/_Project/SO/Weapon",
            "Assets/_Project/SO/Enemy",
            "Assets/_Project/SO/Rewards"
        };

        int deletedCount = 0;

        foreach (string folder in targetFolders)
        {
            if (!Directory.Exists(folder)) continue;

            // [핵심 Fix] 파일 시스템에서 직접 파일을 찾습니다.
            string[] files = Directory.GetFiles(folder, "*.asset");

            foreach (string filePath in files)
            {
                // [핵심 Fix] 윈도우 경로(\)를 유니티 경로(/)로 바꿔서 비교해야 합니다.
                string unityPath = filePath.Replace("\\", "/");

                // 이번 Sync에서 등록되지 않은 파일이면 삭제
                if (!_processedAssets.Contains(unityPath))
                {
                    string fileName = Path.GetFileName(unityPath);
                    if (fileName.StartsWith("Char_") || fileName.StartsWith("Weapon_") ||
                        fileName.StartsWith("Enemy_") || fileName.StartsWith("Reward_"))
                    {
                        AssetDatabase.DeleteAsset(unityPath);
                        Debug.LogWarning($"[Cleanup] 삭제됨: {fileName}");
                        deletedCount++;
                    }
                }
            }
        }

        if (deletedCount > 0)
        {
            Debug.Log($"<color=yellow>총 {deletedCount}개의 미사용 에셋이 정리되었습니다.</color>");
        }
    }

    // ---------------------------------------------------------
    // [헬퍼 함수] 안전한 파싱 (데이터가 없거나 에러나면 기본값 리턴)
    // ---------------------------------------------------------

    string SanitizeFileName(string name)
    {
        string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidFileNameChars());
        foreach(char c in invalidChars)
        {
            name = name.Replace(c.ToString(), "");
        }
        return name.Replace("\"", "").Trim();

    }
    // 문자열 읽기 (없으면 빈 문자열)
    string ParseString(string[] row, int index, string defaultValue = "")
    {
        if (index >= row.Length) return defaultValue;
        string value = row[index].Trim();
        return value = value.Replace("\"", ""); ;
    }

    // 실수 읽기 (없으면 0)
    float ParseFloat(string[] row, int index, float defaultValue = 0f)
    {
        if (index >= row.Length) return defaultValue;
        string value = row[index].Trim();
        value = value.Replace("\"", "");
        if (float.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float result)) return result;
        return defaultValue;
    }

    // 정수 읽기 (없으면 0)
    int ParseInt(string[] row, int index, int defaultValue = 0)
    {
        if (index >= row.Length) return defaultValue;
        string value = row[index].Trim();
        value = value.Replace("\"", "");
        if (int.TryParse(value, out int result)) return result;
        return defaultValue;
    }

    // SO 파일 생성 또는 로드 (연결 유지)
    T FindAndRenameSO<T>(string savePath, string idStr, string newFileName, string prefix) where T : ScriptableObject
    {
        string fullNewPath = $"{savePath}/{newFileName}";

        // 1. 이미 정확한 이름의 파일이 있는지 확인
        T data = AssetDatabase.LoadAssetAtPath<T>(fullNewPath);
        if (data != null) return data;

        // 2. 없다면, 같은 폴더에서 "{Prefix}{ID}_" 로 시작하는 파일이 있는지 검색
        // 예: "Reward_4001_OldName.asset" 을 찾음
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { savePath });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileName(path);

            // 파일명이 접두사+ID로 시작하는지 체크 (예: Reward_4001_)
            if (fileName.StartsWith($"{prefix}{idStr}_"))
            {
                // 찾았다! 이름이 다르다면 리네임 실행
                AssetDatabase.RenameAsset(path, Path.GetFileNameWithoutExtension(newFileName));
                AssetDatabase.SaveAssets();

                // 리네임된 에셋을 로드해서 반환
                return AssetDatabase.LoadAssetAtPath<T>(fullNewPath);
            }
        }

        // 3. 진짜 없으면 새로 생성
        data = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(data, fullNewPath);
        return data;
    }
    void AutoLinkRewardsToManager()
    {
        LevelUpManager manager = FindObjectOfType<LevelUpManager>();

        // 씬에 없으면 프리팹 검색
        if (manager == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab LevelUpManager");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                manager = prefab.GetComponent<LevelUpManager>();
            }
        }

        if (manager != null)
        {
            string folderPath = "Assets/_Project/SO/Rewards";
            string[] guids = AssetDatabase.FindAssets("t:LevelUpRewardSO", new[] { folderPath });

            List<LevelUpRewardSO> allRewards = new List<LevelUpRewardSO>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                LevelUpRewardSO so = AssetDatabase.LoadAssetAtPath<LevelUpRewardSO>(path);
                if (so != null) allRewards.Add(so);
            }

            manager.rewardPool = allRewards;
            EditorUtility.SetDirty(manager);
            Debug.Log($"<color=green>LevelUpManager에 {allRewards.Count}개의 보상 데이터가 자동 연결되었습니다!</color>");
        }
        else
        {
            Debug.LogWarning("LevelUpManager를 찾을 수 없어 자동 연결에 실패했습니다.");
        }
    }

    // 이름으로 프리팹 찾기
    GameObject FindPrefabByName(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName)) return null;
        prefabName = prefabName.Replace("\"", "");
        string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
        return null;
    }
}