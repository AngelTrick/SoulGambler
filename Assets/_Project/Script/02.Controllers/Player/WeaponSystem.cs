using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public enum AttackMode { Auto, SemiAuto, Manual}
public enum WeaponUpgradeType 
{
    None,
    Projectile,
    Speed,
    DamageMultiplier,
    Area,
    Pierce,
    knockBack,
    CoolDown
}
public class WeaponSystem : MonoBehaviour
{
    #region [1] 필수 설정 (Configuration)
    [Header("Weapon Setup")]
    [Tooltip("현재 장착된 무기 데이터 자동 할당 OR 테스트용")] 
    public WeaponDataSO currentWeapon;
    [Tooltip("투사체가 생성될 위치")]
    public Transform firePoint;
    [Tooltip("근거리 공격 히트 박스")]
    public GameObject weaponHitbox;
    #endregion
    #region [2] 전투 설정 (Combat Setting)
    [Space(10)]
    [Header("Combat Setting")]
    [Tooltip("공격 방식 (Auto : 자동 공격 , Manual : 클릭 사격")]
    public AttackMode currentMode = AttackMode.Auto;
    #endregion

    #region [3] 런타임 강화 상태 ( Runtime Debug)
    [Space(20)]
    [Header("Runtime Modifers")]
    [SerializeField, Tooltip("추가 투사체 개수")]
    public int _runBonusProjectileCount = 0;
    
    [SerializeField, Tooltip("추가 공격 속도")]
    public float _runBonusAttackSpeed = 0f;
    
    [SerializeField, Tooltip("추가 관통 수(원거리)")]
    public int _runBonusPirece = 0;

    [SerializeField, Tooltip("범위 증가")]
    public float _runBonusArea = 0f;

    [SerializeField, Tooltip("추가 넉백 힘")]
    public float _runBonusKnockback = 0f;

    [SerializeField, Tooltip("쿨타임 감소율")]
    public float _runBonusCoolDown = 0f;

    [Space(5)]
    [Header ("--- Target Info ---")]
    [SerializeField, Tooltip("현재 조준 중 적")]
    private Transform _target;
    #endregion

    private float _timer = 0f;
    private PlayerInput _playerInput;
    private bool _isFiringPressed = false;
    private bool _isAttacking = false;
    private InputAction _attackAction;
    private Collider[] _targetBuffer = new Collider[20];

    private Vector3 _defaultHiboxScale;
    private void Awake()
    {
        _playerInput = GetComponentInParent<PlayerInput>();
        if (weaponHitbox != null)
        {
            weaponHitbox.SetActive(false);
            _defaultHiboxScale = weaponHitbox.transform.localScale;
        }
        if(_playerInput != null)
        {
            _attackAction = _playerInput.actions["Attack"];
        }
    }

    public void ResetRunBonuses()
    {
        _runBonusProjectileCount = 0;
        _runBonusAttackSpeed = 0f;
        _runBonusArea = 0f;
        _runBonusPirece = 0;
        _runBonusKnockback = 0f;
        _runBonusCoolDown = 0f;

    }

    private void Update()
    {
        if (currentWeapon == null || PlayerController.Instance.IsDead) return;
        if (_playerInput != null) _isFiringPressed = _attackAction.IsPressed();
        else _isFiringPressed = Mouse.current.leftButton.isPressed;
        
        _timer += Time.deltaTime;

        float finalAttackSpeed = currentWeapon.attackSpeed + _runBonusAttackSpeed;
        float basecooldown = 1f / Mathf.Max(0.1f , finalAttackSpeed);

        float cooldownReduceMultiplier = Mathf.Clamp(1f - _runBonusCoolDown, 0.2f, 1f);
        float finalCooldown = basecooldown * cooldownReduceMultiplier;

        if (_timer >= finalCooldown && !_isAttacking)
        {
            if (ShouldFire())
            {
                UpdateTargeting();
                if(_target != null || currentMode == AttackMode.Manual)
                {
                    PerformAttack();
                    _timer = 0f;
                }
            }
        }
    }

    bool ShouldFire()
    {
        switch (currentMode)
        {
            case AttackMode.Auto:
                return true;
            case AttackMode.SemiAuto:
            case AttackMode.Manual:
                return _isFiringPressed;

            default: return true;
        }
    }

    void UpdateTargeting()
    {
        if(currentMode == AttackMode.Manual) _target = null;
        else FindNearestEnemy();
    }
    void FindNearestEnemy()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, currentWeapon.range, _targetBuffer);

        Transform nearest = null;
        float minDistance = Mathf.Infinity;
        for (int i = 0; i < count; i++)
        {
            Collider col = _targetBuffer[i]; // 버퍼에서 꺼내 씀

            // 적 태그 확인 (레이어로 필터링하면 더 좋음)
            if (col.CompareTag("Enemy"))
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = col.transform;
                }
            }
        }
        _target = nearest;
    }
    void PerformAttack()
    {
        if(currentWeapon.weaponType == WeaponType.Ranged) FireRanged();
        else if (currentWeapon.weaponType == WeaponType.Melee) StartCoroutine(FireMelee());
    }
    void FireRanged()
    {
        if (currentWeapon.projectilePrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 baseDir = Vector3.forward;

        if (currentMode == AttackMode.Manual)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) baseDir = sr.flipX ? Vector3.left : Vector3.right;
        }
        else if (_target != null)
        {
            baseDir = (_target.position - spawnPos).normalized;
            baseDir.y = 0;
        }
        else return;

        int totalProjectile = currentWeapon.amount + _runBonusProjectileCount;
        float finalScale = 1.0f + _runBonusArea;
        for(int i = 0;  i < totalProjectile; i++)
        {
            GameObject bulletObj = PoolManager.Instance.Get(currentWeapon.projectilePrefab);
            bulletObj.transform.position = spawnPos;
            bulletObj.transform.rotation = Quaternion.identity;
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();

            if(bulletScript != null)
            {
                float damageMultiplier = (PlayerController.Instance.currentStance == PlayerStance.Dark) ? 1.5f : 1.0f;
                // 여러 발 일 경우 사이 각도 좀 더 벌려 주는 로직 추가 예정
                bulletScript.Init(
                    currentWeapon.projectilePrefab,
                    currentWeapon,
                    damageMultiplier,
                    baseDir,
                    _runBonusPirece,
                    _runBonusKnockback,
                    finalScale
                    );
            }

        }
    }

    IEnumerator FireMelee()
    {
        _isAttacking = true;
        if (weaponHitbox != null)
        {
            float scaleMultiplier = 1.0f + _runBonusArea;
            weaponHitbox.transform.localScale = _defaultHiboxScale * scaleMultiplier;
            weaponHitbox.SetActive(true);
            float duration = 0.3f * (1f - _runBonusCoolDown);
            yield return new WaitForSeconds(0.3f);
            weaponHitbox.SetActive(false);
            weaponHitbox.transform.localScale = _defaultHiboxScale;
        }
        else Debug.LogWarning("근거리 무기용 히트박스가 연결 되지 않았습니다.");

        _isAttacking = false;
    }
    public void CycleMode()
    {
        int nextMode = (int)currentMode + 1;
        if (nextMode > (int)AttackMode.Manual) nextMode = 0;
        currentMode = (AttackMode)nextMode;
        Debug.Log($"공격 모드 변경 : {currentMode}");
    }

    public void ApplyWeaponUpgrade(WeaponUpgradeType type, float value)
    {
        switch (type)
        {
            case WeaponUpgradeType.Projectile:
                _runBonusProjectileCount += (int)value;
                break;
            case WeaponUpgradeType.Speed:
                _runBonusAttackSpeed += value;
                break;
            case WeaponUpgradeType.Area:
                _runBonusArea += value;
                break;
            case WeaponUpgradeType.Pierce:
                _runBonusPirece += (int)value;
                break;
            case WeaponUpgradeType.knockBack:
                _runBonusKnockback += value;
                break;
            case WeaponUpgradeType.CoolDown:
                _runBonusCoolDown += value;
                break;
        }
        Debug.Log($"[Weapon Upgrade] {type} += {value}");
    }
    private void OnDrawGizmosSelected()
    {
        if(currentWeapon != null) 
        {
            Gizmos.color = currentMode == AttackMode.Auto ? Color.yellow : Color.green;
            Gizmos.DrawWireSphere(transform.position, currentWeapon.range);
        }
    }
}
