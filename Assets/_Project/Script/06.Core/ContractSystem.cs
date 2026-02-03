using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

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

    private void Start()
    {
		ShowRandomContract();
    }
	public void ShowRandomContract()
	{
		if (allContracts.Count == 0) return;
		int randomIndex = Random.Range(0, allContracts.Count);
		_currentSelectedContract = allContracts[randomIndex];

		UpdataUI();
	}
	void UpdataUI()
	{
		if (_currentSelectedContract == null) return;

		titleText.text = _currentSelectedContract.contractName;
		descText.text = _currentSelectedContract.description;
		if(_currentSelectedContract.icon != null)
		{
			contractIcon.sprite = _currentSelectedContract.icon;
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
				if(DataManager.instance != null)
				{
					DataManager.instance.currentGameData.totalGold += contract.rewardGold;
				}
				break;
		}
		Debug.Log($"계약 성립 :{contract.contractName} 적용 완료");
	}
	void ApplyStatChange(StatType stat, float value)
	{
		string key = "Bonus_" + stat.ToString();
		float currentValue = PlayerPrefs.GetFloat(key, 0f);
		PlayerPrefs.SetFloat(key, currentValue + value);

		Debug.Log($"스텟 변경 : {stat} {value} (누적: {currentValue + value})");
	}
	void StartGame()
	{
		SceneManager.LoadScene("GameScene");
	}

	
}
