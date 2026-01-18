using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Processors;
using UnityEngine.UIElements;
using DG.Tweening;

public class EnemyController : MonoBehaviour
{
    #region 상태머신 및 선언
    public EnemyStateMachine StateMachine { get; private set; }
    public EnemyIdleState IdleState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }
    #endregion
    public EnemyDataSO enemyData;

    [HideInInspector] public Rigidbody _rb;
    [HideInInspector] public Animator _animator;
    [HideInInspector] public SpriteRenderer _sr;
    [HideInInspector] public Collider _col;

    [HideInInspector] public Transform _target;

    [HideInInspector] public float moveSpeed;
    [HideInInspector] public float attackRange;
    [HideInInspector] public float detectRange;
    [HideInInspector] public float damage;
    [HideInInspector] public float maxHP;
    private float _currentHP;
    private GameObject _originalPrefab;
    private static readonly int HashHit = Animator.StringToHash("Hit");
    private static readonly int HashDead = Animator.StringToHash("Dead");
    

    public float MoveSpeed => enemyData != null ? enemyData.moveSpeed : 3f;
    public float AttackRange => enemyData != null ? enemyData.attackRange : 1f;
    public float DetectRange => enemyData != null ? enemyData.detectRange : 10f;
    public float Damage => enemyData != null ? enemyData.damage : 10f;
    

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        _col = GetComponent<Collider>();
        StateMachine = new EnemyStateMachine();
        IdleState = new EnemyIdleState(this, StateMachine);
        ChaseState = new EnemyChaseState(this, StateMachine);
        AttackState = new EnemyAttackState(this, StateMachine);

    }
    public void Init(GameObject prefab)
    {
        _originalPrefab = prefab;
        if (enemyData != null) _currentHP = enemyData.maxHP;
        if (_col != null) _col.enabled = true;
        if (_sr != null) _sr.color = Color.white;
        if (_rb != null) _rb.velocity = Vector3.zero;

        if (PlayerController.Instance != null)
        {
            _target = PlayerController.Instance.transform;
        }
        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        StateMachine.currentState.LogicUpdate();
    }
    private void FixedUpdate()
    {
        StateMachine.currentState.PhysicsUpdate();
    }
    public void TakeDamage(float damage)
    {
        _currentHP -= damage;

        if (_sr != null)
        {
            _sr.DOKill();
            _sr.color = Color.red;
            _sr.DOColor(Color.white, 0.1f);
        }
        if (_currentHP > 0)
        {
            if (_animator != null) _animator.SetTrigger(HashHit);
        }
        else
        {
            OnDead();
        }

    }
    public void OnDead()
    {
        if (GameManager.Instance != null) GameManager.Instance.AddkillCount();
        if (DataManager.instance != null && enemyData != null) DataManager.instance.AddGold(enemyData.goldReward);

        if (GameManager.Instance != null && GameManager.Instance.expGemPrefab != null)
        {
            GameObject gemobj = Instantiate(GameManager.Instance.expGemPrefab, transform.position, Quaternion.identity);
            ExpGem gemScript = gemobj.GetComponent<ExpGem>();
            if (gemScript != null && _target != null)
            {
                gemScript.Initialize(_target);
            }
        }
        if (_col != null) _col.enabled = false;
        if (_rb != null) _rb.velocity = Vector3.zero;
        if (_animator != null) _animator.SetTrigger(HashDead);

        Invoke("ReturnToPool", 0.5f);
    }
    private void ReturnToPool()
    {
        if (PoolManager.instance != null)
        {
            PoolManager.instance.Return(gameObject, _originalPrefab);
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
