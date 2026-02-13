using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI")]
    public Image hpBar;
    public Image expBar;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    [Header("SkillCoolDown")]
    public Image attackCoolDownImage;
    public Image dashCoolDownImage;
    [Header("Popups")]
    public GameObject gameOverPanel;
    public GameObject levelUpPanel;

    public CanvasGroup levelUpCanvasGroup;
    [Header("Level Up Rewards")]
    public RewardUI[] rewardButtons;


    public void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnEnable()
    {
        if (PlayerController.Instance != null)
            PlayerController.Instance.SubscribeHP(UpdateHP);
        
        if(GameManager.Instance != null)
        {
            GameManager.Instance.SubscribeExp(UpdateExp);
            GameManager.Instance.SubScribeLevel(UpdateLevel);
            GameManager.Instance.SubscribeKillCount(UpdateKillCount);
            GameManager.Instance.SubscribeTimer(UpdateTimer);
        }
    }
    private void OnDisable()
    {
        if (PlayerController.Instance != null)
            PlayerController.Instance.UnsubcribeHP(UpdateHP);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnsubscribeExp(UpdateExp);
            GameManager.Instance.UnsubscribeLevel(UpdateLevel);
            GameManager.Instance.UnsubscribeKillCount(UpdateKillCount);
            GameManager.Instance.UnsubscribeTimer(UpdateTimer);
        }
    }
    public void UpdateHP(float current, float max)
    {
        if (hpBar == null) return;
        float target = (max > 0) ? current / max : 0;

        if (Mathf.Abs(hpBar.fillAmount - target) > 0.01f)
            hpBar.DOFillAmount(target, 0.2f);
        else hpBar.fillAmount = target;
        
    }
    public void UpdateExp(float current,float max)
    {
        if (expBar == null) return;
        expBar.fillAmount = current / max;
    }
    public void UpdateKillCount(int count)
    {
        if (scoreText == null) return;
        scoreText.text = $"Kills : {count}";
        scoreText.transform.DOKill();
        scoreText.transform.localScale = Vector3.one;
        scoreText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 10, 1);
    }
    public void TriggerAttackCoolDown(float time)
    {
        if (attackCoolDownImage == null) return;
        attackCoolDownImage.fillAmount = 1f;
        attackCoolDownImage.DOFillAmount(0f, time).SetEase(Ease.Linear);
    }
    public void TriggerDashCoolDown(float time)
    {
        if (dashCoolDownImage == null) return;
        dashCoolDownImage.fillAmount = 1f;
        dashCoolDownImage.DOFillAmount(0f,time).SetEase(Ease.Linear);
    }
    public void UpdateLevel(int level)
    {
        if (levelText != null) levelText.text = $"Lv.{level}";
    }
    public void UpdateTimer(float time)
    {
        if(timerText != null)
        {
            int min = Mathf.FloorToInt(time / 60f);
            int sec = Mathf.FloorToInt(time % 60f);
            timerText.text = $"{min:00}:{sec:00}";
        }
    }
    public void ShowGameOver() 
    {
        if (gameOverPanel == null) return;
        gameOverPanel.SetActive(true);

        gameOverPanel.transform.localScale = Vector3.zero;
        gameOverPanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
    }
    public void ShowLevelUpUI(bool show)
    {
        if(levelUpPanel == null) return;
        levelUpPanel.SetActive(show);
        if (show)
        {
            RefreshRewardUI();

            if(levelUpCanvasGroup != null)
            {
                levelUpCanvasGroup.interactable = false;
                levelUpCanvasGroup.blocksRaycasts = false;
                levelUpCanvasGroup.alpha = 0.5f;

                StartCoroutine(CoUnlockLevelUp());
            }
        }
    }
    void RefreshRewardUI()
    {
        if (LevelUpManager.Instance == null) return;
        List<RewardOption> rewards = LevelUpManager.Instance.currentRewards;

        if (rewards == null || rewards.Count == 0) return;

        for(int i= 0 ; i < rewardButtons.Length; i++)
        {
            if(i < rewards.Count)
            {
                rewardButtons[i].gameObject.SetActive(true);
                RewardOption option = rewards[i];

                rewardButtons[i].Init(option, () =>
                {
                    LevelUpManager.Instance.SelectReward(option);
                });
            }
            else
            {
                rewardButtons[i].gameObject.SetActive(false);
            }
        }
    }
    public void CloseLevelUpUI()
    {
        ShowLevelUpUI(false);
        Time.timeScale = 1f;
    }
    IEnumerator CoUnlockLevelUp()
    {
        yield return new WaitForSecondsRealtime(3.0f);
        if (levelUpCanvasGroup != null)
        {
            levelUpCanvasGroup.interactable = true;
            levelUpCanvasGroup.blocksRaycasts = true;
            levelUpCanvasGroup.alpha = 1f;
        }
    }

}
