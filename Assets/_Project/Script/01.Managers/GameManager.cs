using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<float, float> OnExpChanged;
    public event Action<int> OnLevelChanged;
    public event Action<int> OnKillCountChanged;
    public event Action<float> OnTimeUpdated;

    [Header("Game Stat")]
    public bool isGameOver = false;
    public int killCount = 0;

    public float gameTime = 0f;

    [Header("Level System")]
    public int level = 1;
    public int currentExp = 0;
    public int maxExp = 100;
    public GameObject expGemPrefab;

    public PlayerController player;

    [Header("Rouguelike Settings")]
    public bool loseGoldOnDeath = true;
    public float goldKeepRatio = 0f;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }
    private void Start()
    {
        DOTween.SetTweensCapacity(2000, 100);
        isGameOver = false;
        gameTime = 0f;
        Time.timeScale = 1f;
        if (player == null) player = FindObjectOfType<PlayerController>();
        OnExpChanged?.Invoke(currentExp, maxExp);
        OnLevelChanged?.Invoke(level);
        OnKillCountChanged?.Invoke(killCount);
        if(PlayerController.Instance != null)
        {
            PlayerController.Instance.OnPlayerDie += OnGameOver;
        }
    }

    //1. 경험치 구독 & 즉시 전송
    public void SubscribeExp(Action<float, float> listener)
    {
        OnExpChanged += listener;
        listener?.Invoke(currentExp, maxExp);
    }
    public void UnsubscribeExp(Action<float, float> listener) => OnExpChanged -= listener;

    //2. 레벨 구족 & 즉시 전송
    public void SubScribeLevel(Action<int> listener)
    {
        OnLevelChanged += listener;
        listener?.Invoke(level);
    }

    public void UnsubscribeLevel(Action<int> listener) => OnLevelChanged -= listener;

    //3. 킬카운트 구족 & 즉시 전송
    public void SubscribeKillCount(Action<int> listener)
    {
        OnKillCountChanged += listener;
        listener?.Invoke(killCount);
    }

    public void UnsubscribeKillCount(Action<int> listener) => OnKillCountChanged -= listener;

    //4. 타이머 구독 & 즉시 전송
    public void SubscribeTimer(Action<float> listener)
    {
        OnTimeUpdated += listener;
        listener?.Invoke(gameTime);
    }

    public void UnsubscribeTimer(Action<float> listener) => OnTimeUpdated -= listener;
    private void Update()
    {
        if (isGameOver || player == null) return;
        gameTime += Time.deltaTime;
        OnTimeUpdated?.Invoke(gameTime);
    }
    private void OnDisable()
    {
        if(PlayerController.Instance != null)
        {
            PlayerController.Instance.OnPlayerDie -= OnGameOver;
        }
    }
    public void GetExp(int amount)
    {
        currentExp += amount;
        if (currentExp >= maxExp) LevelUp();
        OnExpChanged?.Invoke(currentExp, maxExp);
    }
    void LevelUp()
    {
        level++;
        currentExp = 0;
        maxExp += 50;

        OnLevelChanged?.Invoke(level);
        if(LevelUpManager.Instance != null)
        {
            List<RewardOption> rewards = LevelUpManager.Instance.GetRandomRewards();
        }

        if(UIManager.Instance != null)
        {
            UIManager.Instance.ShowLevelUpUI(true);
        }
        Time.timeScale = 0f;
    }
    public void SelectAugment(int type)
    {
        if (player == null) return;
        if(LevelUpManager.Instance != null)
        {
            LevelUpManager.Instance.SelectRewardByIndex(type - 1);
        }
        else
        {
            Debug.LogError("LevelUpManager가 없습니다.");
            if (UIManager.Instance != null)
                UIManager.Instance.ShowLevelUpUI(false);
            Time.timeScale = 1f;
        }
    }
    public void AddkillCount()
    {
        killCount++;
        OnKillCountChanged?.Invoke(killCount);
    }
    public void OnGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        DataManager.Instance.SaveGame();
        UIManager.Instance.ShowGameOver();
    }
    public void RetryGame()
    {
        DG.Tweening.DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void GoToMain()
    {
        DG.Tweening.DOTween.KillAll();
        SceneManager.LoadScene("MainMenu");
    }

}
