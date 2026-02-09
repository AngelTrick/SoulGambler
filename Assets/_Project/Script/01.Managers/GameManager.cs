using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
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
        if(UIManager.Instance != null)
        {
            if (player != null)
                UIManager.Instance.UpdateHP(player.CurrentHP, player.playerData.maxHP);
            UIManager.Instance.UpdateExp(0, maxExp);
            UIManager.Instance.UpdateLevel(level);
            UIManager.Instance.ShowLevelUpUI(false);
            UIManager.Instance.UpdateKillCount(0);
        }
        if(PlayerController.Instance != null)
        {
            PlayerController.Instance.OnPlayerDie += OnGameOver;
        }
    }
    private void Update()
    {
        if (isGameOver || player == null) return;
        gameTime += Time.deltaTime;
        if(UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(player.CurrentHP, player.playerData.maxHP);
            UIManager.Instance.UpdateTimer(gameTime);
        }
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
        UIManager.Instance.UpdateExp(currentExp, maxExp);
        if (currentExp >= maxExp) LevelUp();
    }
    void LevelUp()
    {
        level++;
        currentExp = 0;
        maxExp += 50;
        if(LevelUpManager.Instance != null)
        {
            List<RewardOption> rewards = LevelUpManager.Instance.GetRandomRewards();
        }

        if(UIManager.Instance != null)
        {
            UIManager.Instance.UpdateLevel(level);
            UIManager.Instance.UpdateExp(0, maxExp);
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
        if(UIManager.Instance != null)
            UIManager.Instance.UpdateKillCount(killCount);
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
