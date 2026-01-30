using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;



public class ContractSystem : MonoBehaviour
{
	[Header("UI")]
	public GameObject contractPanel;
	public GameObject dicePanel;
	public TMP_InputField nameInput;
	public TextMeshProUGUI diceResultText;
	public TextMeshProUGUI totalGoldText;
	[Header("주사위 연출")]
	public Image diceImage;
	public Sprite[] diceSprites;
	private void Start()
	{
		if (DataManager.instance != null && totalGoldText != null)
		{
			int gold = DataManager.instance.currentGameData.totalGold;
			totalGoldText.text = $"TOTAL GOLD : {gold:N0}";
		}
	}

	public void OnClickExit()
	{
		Application.Quit();
	}
	public void OnSignContract()
	{
		string nickName = nameInput.text;
		if (string.IsNullOrEmpty(nickName)) return;
		PlayerPrefs.SetString("PlayerName", nickName);

		if(DataManager.instance != null)
		{
			DataManager.instance.currentGameData.playerName = nickName;
		}
		StartCoroutine(RollDiceAndStart());
	}
	IEnumerator RollDiceAndStart()
	{
		if (contractPanel != null) contractPanel.SetActive(false);
		if (dicePanel != null) dicePanel.SetActive(true);

		if(diceResultText != null) diceResultText.text = "";

		diceImage.transform.DOShakePosition(2.0f, 30f, 20, 90, false, true);
		float duration = 2.0f;
		float timer = 0f;

		while(timer < duration)
		{
			timer += Time.deltaTime;
			int randomIndex = Random.Range(0, 6);
			diceImage.sprite = diceSprites[randomIndex];

			yield return new WaitForSeconds(0.05f);
		}
		int finalDiceValue = Random.Range(1, 7);
		diceImage.sprite = diceSprites[finalDiceValue - 1];
		if (diceResultText != null)
		{
			diceResultText.text = $"<color=red>ATK +{finalDiceValue}</color>";
			diceResultText.transform.localScale = Vector3.zero;
			diceResultText.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack);
		}

		// 4. 보너스 저장 (우체통에 넣기)
		PlayerPrefs.SetInt("BonusDamage", finalDiceValue);
		PlayerPrefs.Save();

		Debug.Log($"[계약 성립] 이름: {nameInput.text} / 보너스: {finalDiceValue}");

		yield return new WaitForSeconds(3f);

		// 5. 게임 씬으로 출발!
		SceneManager.LoadScene("GameScene");
	}
}
