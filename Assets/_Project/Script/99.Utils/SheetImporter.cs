// ==========================================
// FILE NAME: SheetImporter.cs
// 설명: 구글 시트 CSV 파서 (안전 장치 및 CSV 포맷 자동 매칭 적용)
// ==========================================

using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SheetImporter : EditorWindow
{
    public TextAsset characterCsv;
    public TextAsset weaponCsv;
    public TextAsset enemyCsv;

    [MenuItem("Tools/Data Importer")]
    public static void ShowWindow()
    {
        GetWindow<SheetImporter>("Data Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("Google Sheet CSV Importer (Final Ver)", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        characterCsv = (TextAsset)EditorGUILayout.ObjectField("Character Data", characterCsv, typeof(TextAsset), false);
        weaponCsv = (TextAsset)EditorGUILayout.ObjectField("Weapon Data", weaponCsv, typeof(TextAsset), false);
        enemyCsv = (TextAsset)EditorGUILayout.ObjectField("Enemy Data", enemyCsv, typeof(TextAsset), false);

        EditorGUILayout.Space();

        if (GUILayout.Button("Import / Update Data", GUILayout.Height(40)))
        {
            if (characterCsv != null) ImportCharacters();
            if (weaponCsv != null) ImportWeapons();
            if (enemyCsv != null) ImportEnemies();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("모든 데이터 동기화 완료!");
        }
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

    // ---------------------------------------------------------
    // [1] 캐릭터 데이터 파싱 (10개 열)
    // ---------------------------------------------------------
    void ImportCharacters()
    {
        string savePath = "Assets/_Project/SO/Characters";
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

        string[] lines = characterCsv.text.Replace("\r\n", "\n").Split('\n');

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
    void ImportWeapons()
    {
        string savePath = "Assets/_Project/SO/Weapon";
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

        string[] lines = weaponCsv.text.Replace("\r\n", "\n").Split('\n');

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
    void ImportEnemies()
    {
        string savePath = "Assets/_Project/SO/Enemy";
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

        string[] lines = enemyCsv.text.Replace("\r\n", "\n").Split('\n');

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
}