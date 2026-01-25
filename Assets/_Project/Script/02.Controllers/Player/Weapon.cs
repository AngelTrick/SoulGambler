using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // 플레이어 정보 가져오기
            float realDamage = 0f;

            if (PlayerController.Instance != null)
            {
                realDamage = PlayerController.Instance.GetFinalDamage();
            }
            else
            {
                Debug.LogWarning("PlayerController Instance가 없습니다! 데미지가 0으로 들어갑니다.");
            }

            // 적에게 데미지 주기
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(realDamage);
                Debug.Log($" 타격 성공! 적: {other.name} / 피해량: {realDamage}");
            }
        }
    }
}
