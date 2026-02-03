using System.Collections;
using System.Collections.Generic;
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
    [Header("Data")]
    public PlayerDataSO playerData;

    [Header("Stances")]
    public PlayerStance currentStance = PlayerStance.Light;
    public Color lightColor = Color.white;
    public Color darkColor = new Color(0.8f, 0.4f, 1f,1f);
    [HideInInspector] public float currentMaxHP;
    [HideInInspector] public float currentMoveSpeed;
    [HideInInspector] public float currentDamage;
    [HideInInspector] public float currentAttackCoolDown;
    [HideInInspector] public float currentDefense;
    [HideInInspector] public float currentCritChance;
  
    private float _currentHP;
    public float CurrentHP => _currentHP;
    [Header("Combat")]
    public GameObject weaponHitbox;
    [Header("Component")]
    private Rigidbody _rb;
    private SpriteRenderer _sr;
    private Animator _anim;
    private PlayerInput _playerInput;
    private Vector3 _moveDir;

    private bool _isDashing = false;
    private bool _isAttacking = false;
    private bool _isDead = false;
    private bool _canDash = true;
    private bool _isHit = false;
    private bool _isInvincible = false;
    private bool IsMoving;
    
    public  bool IsDashing => _isDashing;
    public bool IsAttacking => _isAttacking;
    public  bool IsDead => _isDead;

    private Transform _cameraTransform;

    private void Awake()
    {
        Instance = this;
        _playerInput = GetComponent<PlayerInput>();
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponentInChildren<Animator>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        if (playerData != null)
        {
            InitializeStats();
        }
        else
        {
            Debug.LogError("PlayerDataSO가 연결 안되어 있습니다.");
        }
        if(Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }
    }
    private void Start()
    {
        if(weaponHitbox != null) weaponHitbox.SetActive(false);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(_currentHP, currentMaxHP);
        }
        ApplyStanceEffect();

    }
    void InitializeStats()
    {
        currentMaxHP = playerData.maxHP;
        currentMoveSpeed = playerData.moveSpeed;
        currentDamage = playerData.damage;
        currentAttackCoolDown = playerData.attackCooldown;
        currentDefense = playerData.defense;
        currentCritChance = playerData.critChance;

        if(DataManager.instance != null && DataManager.instance.currentGameData != null)
        {
            GameData data = DataManager.instance.currentGameData;
            currentDamage += data.attackLevel * 2f ;
            currentMaxHP += data.healthLevel * 10f;
            currentMoveSpeed += data.speedLevel * 0.1f;
            currentDefense += data.defenseLevel * 1f;
            currentCritChance += data.critChanceLevel * 1f;

            int bonusDmg = PlayerPrefs.GetInt("BonusDamage", 0);
            currentDamage += bonusDmg;
        }
        _currentHP = currentMaxHP;
        Debug.Log($"기본 공격력 : {currentDamage} HP : {currentMaxHP} , Crit : {currentCritChance}");
        
    }
    private void Update()
    {
        if (_isDead) return;
        if (_isHit)
        {
            _moveDir = Vector3.zero;
        }

        Vector2 inputVec = _playerInput.actions["Move"].ReadValue<Vector2>();

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
        if (_playerInput.actions["Dash"].WasPerformedThisFrame())
        {
            TryDash();
        }
        /*if (_playerInput.actions["Attack"].WasPerformedThisFrame())
        {
            TryAttack();
        }
        */
        if (Keyboard.current.tabKey.wasPressedThisFrame) SwapStance();
        UpdateVisuals();
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
        {
            currentSpeed = playerData.dashSpeed;
        }
        else if (!_isDashing)
        {
            currentSpeed = playerData.moveSpeed;
        }
        _rb.MovePosition(_rb.position + _moveDir * currentSpeed * Time.fixedDeltaTime);
    }
    public void SwapStance()
    {
        if (currentStance == PlayerStance.Light)
        {
            currentStance = PlayerStance.Dark;
        }
        else currentStance = PlayerStance.Light;
        ApplyStanceEffect();
    }
    void ApplyStanceEffect()
    {
        if (_sr != null) return;
        if (currentStance == PlayerStance.Light)
        {
            _sr.color = lightColor;
        }
        else _sr.color = darkColor;
    }
    public float GetFinalDamage()
    {
        float damage = currentDamage;

        if(Random.Range(0f,100f)< currentCritChance)
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
    /*private void TryAttack()
    {
        if (_isAttacking || _isDashing) return;
        StartCoroutine(CoAttack());
    private IEnumerator CoAttack()
    {
        _isAttacking = true;
        _moveDir = Vector3.zero;
        _anim.SetTrigger("Attack");
        if (UIManager.Instance != null) UIManager.Instance.TriggerAttackCoolDown(currentAttackCoolDown);
        if (weaponHitbox != null) weaponHitbox.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        if (weaponHitbox != null) weaponHitbox.SetActive(false);
        yield return new WaitForSeconds(playerData.attackCooldown);
        _isAttacking = false;
        yield return null; 
    }
    }*/
    public void TakeDamage(float damage)
    {
        if (_isDead || _isDashing || _isInvincible) return;
        float damageAfterDefense = Mathf.Max(1, damage - currentDefense);
        float finalDamage = damageAfterDefense;
        if(currentStance == PlayerStance.Light)
        {
            finalDamage *= 0.7f;
        }
        _currentHP -= finalDamage;
        Debug.Log($"피격 데미지 {finalDamage}(태세 : {currentStance}) /남은 체력 : {_currentHP}");
        if (UIManager.Instance != null) UIManager.Instance.UpdateHP(_currentHP, currentMaxHP);
        if(_currentHP <= 0)
        {
            OnDie();
        }
        else
        {
            StartCoroutine(CoHIt());
        }
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
        StopAllCoroutines();
        _anim.SetTrigger("Dead");
        _sr.color = Color.white;
        _rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null) spawner.enabled = false;
        var enimies = FindObjectsOfType<EnemyController>();
        foreach (var e in enimies) e.enabled = false;
        Debug.Log("Player Dead!");
        StartCoroutine(CoDead());
    }
    private IEnumerator CoDead()
    {
        yield return new WaitForSeconds(0.4f);
        GameManager.Instance.OnGameOver();
        gameObject.SetActive(false);
    }
    private void UpdateVisuals()
    {
        if (_isDead) return;
        IsMoving = _moveDir.magnitude > 0.01f;
        if(_anim != null)
        {
            _anim.SetBool("IsRun", IsMoving);
        }
        if (_moveDir.x > 0)
        {
            _sr.flipX = false;
            if (weaponHitbox != null)
            { weaponHitbox.transform.localRotation = Quaternion.Euler(0, 0, 0); }
        }
        else if (_moveDir.x < 0)
        {
            _sr.flipX = true;
            if (weaponHitbox != null)
            {weaponHitbox.transform.localRotation = Quaternion.Euler(0, 180, 0); }
        }
    }
    public void FullRecovery()
    {
        _currentHP = currentMaxHP;
        Debug.Log("플레이어 체력 완전 회복");
    }

    public float GetFinalMagnetRange()
    {
        float range = playerData.magnet;

        if(DataManager.instance != null && DataManager.instance.currentGameData != null)
        {
            range += DataManager.instance.currentGameData.magnetLevel * 0.5f;
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
                ApplyWeaponReward(reward.weaponData);
                break;
        }
    }
    void ApplyStatReward(StatType statType, float value)
    {
        switch (statType) 
        {
            case StatType.MaxHP:
                currentMaxHP += value;
                _currentHP += value;
                if (UIManager.Instance != null) UIManager.Instance.UpdateHP(_currentHP, currentMaxHP);
                break;
            case StatType.Damage:
                currentDamage += value;
                break;
            case StatType.MoveSpeed:
                currentMoveSpeed += value;
                break;
            case StatType.Defense:
                currentDefense += value;
                break;
            case StatType.CritChance:
                currentCritChance += value;
                break;
        }
        Debug.Log($"[Reward] 스탯 강화 : Type {statType}, Value {value}");
    }
    void ApplyWeaponReward(WeaponDataSO weaponData)
    {
        WeaponSystem waeponSys = GetComponent<WeaponSystem>();
        currentDamage += 2f;
        Debug.Log($"[Reward] 무기 강화 완료 {weaponData.weaponName}(공격력 증가)");
    }
}
