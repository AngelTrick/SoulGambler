using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image iconImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI valueText;
    public Button selectButton;

    private int _index;
    
    public void Init(int index, RewardOption option)
    {
        _index = index;

        if (titleText != null) titleText.text = option.title;
        if (descText != null) descText.text = option.description;

        if(iconImage != null)
        {
            if(option.icon != null)
            {
                iconImage.sprite = option.icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => OnClickReward());
    }
    void OnClickReward()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.SelectAugment(_index - 1);
        }
    }
}
