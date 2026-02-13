using TMPro;
using UnityEngine;
using DG.Tweening;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private GameObject _originalPrefab; // 풀 반납용 원본 프리팹 참조

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    // 데미지 텍스트 설정 (외부에서 설정)
    public void Setup(float damageAmount,GameObject originalPrefab, bool isCritical = false)
    {
        _originalPrefab = originalPrefab;
        // 1. 텍스트 설정
        textMesh.text = Mathf.RoundToInt(damageAmount).ToString();

        // 2. 색상 및 크기 설정 (크리티컬 여부에 따른)
        if (isCritical)
        {
            textMesh.fontSize = 6f;
            textMesh.color = Color.yellow;
        }
        else
        {
            textMesh.fontSize = 4f;
            textMesh.color = Color.white;
        }

        // 3. 애니메이션 (Dotween 활용)
        // 위로 1.5만큼 0.5초 동안 이동
        transform.DOMoveY(transform.position.y + 1.5f, 0.5f).SetEase(Ease.OutQuad);

        // 0.3초 대기 후 -> 0.2초 동안 투명해짐 -> 끝나면 풀로 반납
        textMesh.DOFade(0, 0.2f)
            .SetDelay(0.3f)
            .OnComplete(() => ReturnToPool());
    }

    private void ReturnToPool()
    {
        //색상 초기화 (다음 사용을 위해)
        textMesh.color = Color.white;
        textMesh.alpha = 1f;

        //풀 매니저 반납
        if (PoolManager.Instance != null) PoolManager.Instance.Return(gameObject, _originalPrefab);
        else Destroy(gameObject);
    }
}
