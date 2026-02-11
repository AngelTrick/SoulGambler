using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image iconImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI valueText;
    public Button selectButton;

    private Action _onClickCallBack;  
    
    public void Init(RewardOption option, Action onClick)
    {
        //UI °»½Å
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

        _onClickCallBack = onClick;
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => OnClickReward());
    }
    void OnClickReward()
    {
        _onClickCallBack?.Invoke();
    }
}
