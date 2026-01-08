using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager instance;
    public GameObject enemyPrefab;

    private List<GameObject> _pools = new List<GameObject>();
    public void Awake()
    {
        instance = this;
    }
    public GameObject Get()
    {
        GameObject select = null;
        foreach (GameObject item in _pools)
        {
            if (!item.activeSelf)
            {
                select = item;
                select.SetActive(true);
                break;
            }
        }
        if( select == null)
        {
            select = Instantiate(enemyPrefab,transform);
            _pools.Add(select);
        }
        return select;
    }
}
