using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class EnemySpawner : MonoBehaviour
{
    public float spawnTIme = 1f;
    private float timer = 0f;

    private void Update()
    {
        timer += Time.deltaTime;
        if(timer > spawnTIme)
        {
            timer = 0f;
            spawn();
        }
    }
    void spawn()
    {
        GameObject enemy = PoolManager.instance.Get();
        enemy.transform.position = transform.position;
    }
}


