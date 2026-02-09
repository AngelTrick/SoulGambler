using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
public enum PlayerStance { Light, Dark};

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    #region [1] 설정 및 데이터 (Configuration)
    [Header("Core Setting")]
    [Tooltip ("플레이어 기본 스텟 담긴 SO 파일")]
    public PlayerDataSO playerData;
    [Space(10)]
    [Header("Visual & Stance")]
    [Tooltip ("현재 플레이어의 태세 (TAB 키 전환)")]
    public PlayerStance currentStance = PlayerStance.Light;
    [Tooltip("빛 상태 플레이어 색상")]
    public Color lightColor = Color.white;
    [Tooltip("어둠 상태 플레이어 색상")]
    public Color darkColor = new Color(0.8f, 0.4f, 1f,1f);
    #endregion
    #region [2] 런타임 디버깅용(RunTime Debug)
    [Space(20)]
    [Header("RunTime State (Do Not Edit)")]
    [SerializeField , Tooltip("현재 체력")]
    [ReadOnly]private float _currentHP;

    // 프로퍼티는 인스펙터에 확인 불가능 , 확인 필요한 변수만 열람
    [Space(5)]
    [Header("--- Level Up Bonuses (Reste on Die) ---")]
    [SerializeField] private float _runBounsHP = 0f;
    [SerializeField] private float _runBounsDamage = 0f;
    [SerializeField] private float _runBounsSpeed = 0f;
    [SerializeField] private float _runBounsDefense = 0f;
    [SerializeField] private float _runBounsCritChance = 0f;

    [Space(5)]
    [Header ("---Flags---")]
    [SerializeField, Tooltip("대쉬 중인가?")]private bool _isDashing = false;
    [SerializeField, Tooltip("공격 중인가?")] private bool _isInvincible = false;
    [SerializeField, Tooltip("사망 상태인가?")] private bool _isDead = false;
    #endregion

    //======================================================
    // [외부 공개 프로퍼티] 다른 스크립트에서 이 값들 참조 
    //======================================================
    public float CurrentHP => _currentHP;
    public float currentMaxHP => _startMaxHp + _runBounsHP;
    public float currentMoveSpeed => _startMoveSpeed + _runBounsSpeed;
    public float currentDamage => _startDamage + _runBounsDamage;
    public float currentDefense => _startDefense + _runBounsDefense;
    public float currentCritChance => _startCritChance + _runBounsCritChance;
  
    [HideInInspector] public float currentAttackCoolDown;

    //내부 계산용 시작 스탯 
    private float _startMaxHp;
    private float _startMoveSpeed;
    private float _startDamage;
    private float _startDefense;
    private float _startCritChance;

    //컴포넌트 캐싱
    private Rigidbody _rb;
    private SpriteRenderer _sr;
    private Animator _anim;
    private PlayerInput _playerInput;
    private Transform _cameraTransform;

    private Vector3 _moveDir;
    private bool _isAttacking = false;
    private bool _canDash = true;
    private bool _isHit = false;
    private bool IsMoving;
    
    private InputAction _moveAction;
    private InputAction _dashAction;

    public  bool IsDashing => _isDashing;
    public bool IsAttacking => _isAttacking;
    public  bool IsDead => _isDead;


    public event System.Action OnPlayerDie;
    //private InputAction _attackAction;
    private void Awake()
    {
        Instance = this;
        _playerInput = GetComponent<PlayerInput>();
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponentInChildren<Animator>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        if (playerData != null)
            InitializeStats();
        else
            Debug.LogError("PlayerDataSO가 연결 안되어 있습니다.");
        if(Camera.main != null)
            _cameraTransform = Camera.main.transform;
        if(_playerInput != null)
        {
            _moveAction = _playerInput.actions["Move"];
            _dashAction = _playerInput.actions["Dash"];
        }
    }
    private void Start()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateHP(_currentHP, currentMaxHP);
        ApplyStanceEffect();
    }
    void InitializeStats()
    {
        // 1. 기본값
        _startMaxHp = playerData.maxHP;
        _startMoveSpeed = playerData.moveSpeed;
        _startDamage = playerData.damage;
        currentAttackCoolDown = playerData.attackCooldown;
        _startDefense = playerData.defense;
        _startCritChance = playerData.critChance;

        // 2. 영구 강화 적용
        if(DataManager.Instance != null && DataManager.Instance.currentGameData != null)
        {
            GameData data = DataManager.Instance.currentGameData;
            _startMaxHp += data.healthLevel * 10f;
            _startMoveSpeed += data.speedLevel * 0.1f;
            _startDamage += data.attackLevel * 2f ;
            _startDefense += data.defenseLevel * 1f;
            _startCritChance += data.critChanceLevel * 1f;

            _startMaxHp += data.GetContractBonusValue(StatType.MaxHP);
            _startMoveSpeed += data.GetContractBonusValue(StatType.MoveSpeed);
            _startDamage += data.GetContractBonusValue(StatType.Damage);
            _startDefense += data.GetContractBonusValue(StatType.Defense);
            _startCritChance += data.GetContractBonusValue(StatType.CritChance);
        }

        // 3. 런타임 보너스 초기화
        _runBounsHP = 0f;
        _runBounsSpeed = 0f;
        _runBounsDamage = 0f;
        _runBounsDefense = 0f;
        _runBounsCritChance = 0f;

        _currentHP = currentMaxHP;
        Debug.Log($"기본 공격력 : {currentDamage} HP : {currentMaxHP} , Crit : {currentCritChance}");
        
    }
    private void Update()
    {
        if (_isDead) return;
        if (_isHit)
            _moveDir = Vector3.zero;

        Vector2 inputVec = Vector2.zero;
        if(_moveAction != null)
            inputVec = _moveAction.ReadValue<Vector2>();

        if (!_isAttacking && !_isDashing)
        {
            if(_cameraTransform != null)
            {
                Vector3 camForward = _cameraTransform.forward;
                Vector3 camRight = _cameraTransform.right;
                camForward.y = 0;
                camRight.y = 0;
                camForward.Normalize();
                camRight.Normalize();
                _moveDir = (camForward * inputVec.y + camRight * inputVec.x).normalized;
            }
            else
            {
                _moveDir = new Vector3(inputVec.x, 0, inputVec.y);
            }
            if (inputVec.magnitude < 0.1f) _moveDir = Vector3.zero;
        }
        if (_dashAction != null && _dashAction.WasPerformedThisFrame())
            TryDash();
        if (Keyboard.current.tabKey.wasPressedThisFrame) SwapStance();
        UpdateVisuals();
        // 테스트 용 자살키 (추후 완성시 삭제)
        if (Keyboard.current.kKey.wasPressedThisFrame) OnDie();
    }
    private void FixedUpdate()
    {
        if (_isDead) return;
        Move();
    }
    private void Move()
    {
        if (_isDead || _isHit) return;
        float speedMultiplier = (currentStance == PlayerStance.Dark) ? 1.2f : 1.0f; 
        float currentSpeed = playerData.moveSpeed * speedMultiplier;
        if (_isDashing)
            currentSpeed = playerData.dashSpeed;
        else if (!_isDashing)
            currentSpeed = playerData.moveSpeed;
        _rb.MovePosition(_rb.position + _moveDir * currentSpeed * Time.fixedDeltaTime);
    }
    public void SwapStance()
    {
        currentStance = (currentStance == PlayerStance.Light) ? PlayerStance.Dark : PlayerStance.Light;
        ApplyStanceEffect();
    }
    void ApplyStanceEffect()
    {
        if (_sr != null) return;
        _sr.color = (currentStance == PlayerStance.Light) ? lightColor : darkColor;
    }
    public float GetFinalDamage()
    {
        float damage = currentDamage;

        if(Random.Range(0f,100f) < currentCritChance)
        {
            damage *= playerData.critDamage;
        }
        if(currentStance == PlayerStance.Dark)
        {
            damage *= 1.5f;
        }
        return damage;
    }
    private void TryDash()
    {
        if (_isDashing || _isAttacking || !_canDash || _isDead) return;
        StartCoroutine(CoDash());
    }
    private IEnumerator CoDash()
    {
        _isDashing = true;
        _canDash = false;
        _anim.SetTrigger("Dash");
        float totalCooldown = playerData.dashDuration + playerData.dashCooldown;
        if (UIManager.Instance != null) UIManager.Instance.TriggerDashCoolDown(totalCooldown);

        yield return new WaitForSeconds(playerData.dashDuration);
        _isDashing = false;

        yield return new WaitForSeconds(playerData.dashCooldown);
        _canDash = true;
        yield return null;
    }
    public void TakeDamage(float damage)
    {
        if (_isDead || _isDashing || _isInvincible) return;

        float damageAfterDefense = Mathf.Max(1, damage - currentDefense);
        float finalDamage = damageAfterDefense;
        if(currentStance == PlayerStance.Light) finalDamage *= 0.7f;

        _currentHP -= finalDamage;

        if (UIManager.Instance != null) UIManager.Instance.UpdateHP(_currentHP, currentMaxHP);
        
        if(_currentHP <= 0) OnDie();
        else StartCoroutine(CoHIt());
    }
    private IEnumerator CoHIt()
    {
        _isHit = true;
        _isInvincible = true;
        _anim.SetTrigger("Hit");
        _sr.color = Color.red;
        _isAttacking = false;
        _isDashing = false;
        
        yield return new WaitForSeconds(0.4f);
        _isHit = false;
        
        Color invincColor = (currentStance == PlayerStance.Light) ? lightColor : darkColor;
        invincColor.a = 0.8f;
        _sr.color = invincColor;

        yield return new WaitForSeconds(0.6f);
        _isInvincible = false;
        ApplyStanceEffect();
    }
    public void OnDie()
    {
        if (_isDead) return;
        _isDead = true;
        OnPlayerDie?.Invoke();
        StopAllCoroutines();
        _anim.SetTrigger("Dead");
        _sr.color = Color.white;
        _rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null) spawner.enabled = false;
        Debug.Log("Player Dead!");
        StartCoroutine(CoDead());
    }
    private IEnumerator CoDead()
    {
        yield return new WaitForSeconds(0.4f);
        if(GameManager.Instance != null) GameManager.Instance.OnGameOver();
        gameObject.SetActive(false);
    }
    private void UpdateVisuals()
    {
        if (_isDead) return;
        IsMoving = _moveDir.magnitude > 0.01f;
        if(_anim != null) _anim.SetBool("IsRun", IsMoving);
        if (_moveDir.x > 0) _sr.flipX = false;
        else if (_moveDir.x < 0) _sr.flipX = true;
    }
    public void FullRecovery()
    {
        _currentHP = currentMaxHP;
    }

    public float GetFinalMagnetRange()
    {
        float range = playerData.magnet;

        if(DataManager.Instance != null && DataManager.Instance.currentGameData != null)
        {
            range += DataManager.Instance.currentGameData.magnetLevel * 0.5f;
        }
        //range += itemBonusRange;
        return range;
    }
    //========================================================
    // [New] 레벨업 보상 적용 함수 (LevelUpManager에서 호출)
    //========================================================
    public void ApplyReward(RewardOption reward)
    {
        switch (reward.type)
        {
            case RewardType.StatUp:
                ApplyStatReward(reward.statType, reward.statValue);
                break;
            case RewardType.NewWeapon:
            case RewardType.UpgradeWeapon:
                if(GetComponent<WeaponSystem>() != null)
                {
                    GetComponent<WeaponSystem>().ApplyWeaponUpgrade(reward.weaponUpgradeType, reward.statValue);
                }
                break;
        }
    }
    void ApplyStatReward(StatType statType, float value)
    {
        switch (statType) 
        {
            case StatType.MaxHP:
                _runBounsHP += value;
                _currentHP += value;
                if (UIManager.Instance != null) UIManager.Instance.UpdateHP(_currentHP, currentMaxHP);
                break;
            case StatType.Damage:
                _runBounsDamage += value;
                break;
            case StatType.MoveSpeed:
                _runBounsSpeed += value;
                break;
            case StatType.Defense:
                _runBounsDefense += value;
                break;
            case StatType.CritChance:
                _runBounsCritChance += value;
                break;
        }
        Debug.Log($"[Reward] 스탯 강화 : Type {statType}, Value {value}");
    }
}