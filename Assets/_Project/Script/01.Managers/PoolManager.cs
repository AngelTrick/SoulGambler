using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    private Dictionary<int,Queue<GameObject>> poolDictionary  = new Dictionary<int, Queue<GameObject>>();
    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnSceneLoaded(Scene scene,LoadSceneMode mode)
    {
        poolDictionary.Clear();
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public GameObject Get(GameObject prefab)
    {
        int key = prefab.GetInstanceID();
        if (!poolDictionary.ContainsKey(key))
        {
            poolDictionary.Add(key, new Queue<GameObject>());
        }
        Queue<GameObject> queue = poolDictionary[key];
        GameObject select = null;
        if (queue.Count > 0)
        {
            select = queue.Dequeue();
            if (select == null)
            {
                select = Instantiate(prefab, transform);
            }
            else
            {
                select.SetActive(true);
            }
        }
        else
        {
            select = Instantiate(prefab,transform);
        }
        return select;
    }
    public void Return(GameObject obj, GameObject originalPrefab)
    {
        int key = originalPrefab.GetInstanceID();
        if (poolDictionary.ContainsKey(key))
        {
            obj.SetActive(false);
            poolDictionary[key].Enqueue(obj);
        }
        else
        {
            Destroy(obj);
        }
    }
}
