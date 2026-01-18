using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
[System.Serializable]
public struct Wave
{
    public string waveName;
    public float waveDuration;
    public float spawnInterval;
}
public class EnemySpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    public Wave[] waves;
    public float spawnRadius = 10f;

    public GameObject enemyPrefab;

    private int currentWaveIndex = 0;
    private float waveTimer = 0f;
    private float spawnTImer = 0f;

    private Transform player;
    private void Start()
    {
        if (PlayerController.Instance != null)
            player = PlayerController.Instance.transform;
    }
    private void Update()
    {
        if (currentWaveIndex >= waves.Length) return;

        waveTimer += Time.deltaTime;
        spawnTImer += Time.deltaTime;

        Wave currentWave = waves[currentWaveIndex];

        if(spawnTImer >= currentWave.spawnInterval)
        {
            spawnTImer = 0f;
            SpawnEnemy();
        }
        if(waveTimer >= currentWave.waveDuration)
        {
            currentWaveIndex++;
            waveTimer = 0f;
            Debug.Log($"웨이브 변경 ! 현재 웨이브 : {currentWaveIndex}");
        }
    }
    void SpawnEnemy()
    {
        GameObject enemy = PoolManager.instance.Get(enemyPrefab);
        Vector3 spawnPos = Vector3.zero;
        if (player != null)
        {
            Vector2 randomPoint = Random.insideUnitCircle.normalized * spawnRadius;
            spawnPos = player.position + new Vector3(randomPoint.x, 0, randomPoint.y);
        }
        enemy.transform.position = spawnPos;

        EnemyController enemyScript = enemy.GetComponent<EnemyController>();
        if(enemyScript != null)
        {
            enemyScript.Init(enemyPrefab);
        }
    }
}


