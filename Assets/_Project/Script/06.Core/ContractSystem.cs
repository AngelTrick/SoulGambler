using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ContractSystem : MonoBehaviour
{
	[Header("Data")]
	public List<ContractDataSO> allContracts;
	public ContractDataSO _currentSelectedContract;

	[Header("UI")]
	public GameObject contractPanel;
	public TextMeshProUGUI titleText;
	public TextMeshProUGUI descText;
	public Image contractIcon;
	public TextMeshProUGUI totalGoldText;

	public void Init()
	{
		UpdateTotalGoldUI();
        ShowRandomContract();
	}
    private void Start()
    {
		Init();
    }

	void UpdateTotalGoldUI()
	{
		if(DataManager.Instance != null && totalGoldText != null)
		{
			int gold = DataManager.Instance.TotalGold;
			totalGoldText.text = $"Total Gold : {gold: NO}";
		}
	}
	public void ShowRandomContract()
	{
		if (allContracts.Count == 0) return;
		int randomIndex = Random.Range(0, allContracts.Count);
		_currentSelectedContract = allContracts[randomIndex];

		UpdateUI();
	}
	void UpdateUI()
	{
		if (_currentSelectedContract == null) return;

		if(titleText != null) titleText.text = _currentSelectedContract.contractName;
		if(descText != null) descText.text = _currentSelectedContract.description;
		if(_currentSelectedContract.icon != null)
		{
			contractIcon.sprite = _currentSelectedContract.icon;
			contractIcon.gameObject.SetActive(true);
		}
		else
		{
			contractIcon.gameObject.SetActive(false);
		}
	}
	public void OnAcceptContract()
	{
		ApplyContractEffect(_currentSelectedContract);
		StartGame();
	}
	public void OnRefuseContract()
	{
		Debug.Log("계약 거절 : 기본 상태로 시작합니다.");
		StartGame();
	}
	void ApplyContractEffect(ContractDataSO contract)
	{
		if (contract == null) return;
		ApplyStatChange(contract.costStat, -contract.costValue);

		switch (contract.type)
		{
			case ContractType.StatTrade:
				ApplyStatChange(contract.rewardStat, contract.rewardValue);
				break;
			case ContractType.GoldGrant:
				if(DataManager.Instance != null)
				{
					DataManager.Instance.AddGold(contract.rewardGold);
				}
				break;
		}
		Debug.Log($"계약 성립 :{contract.contractName} 적용 완료");
	}
	void ApplyStatChange(StatType stat, float value)
	{
		if(DataManager.Instance != null && DataManager.Instance.currentGameData != null)
		{
			DataManager.Instance.currentGameData.AddContractBonusValue(stat, value);
			DataManager.Instance.SaveGame();
		}

		Debug.Log($"스텟 변경 : {stat} {value} (통합 저장 완)");
	}
	void StartGame()
	{
		SceneManager.LoadScene("GameScene");
	}

	
}
