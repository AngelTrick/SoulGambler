using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem.Processors;
using UnityEngine.UIElements;

public class EnemyController : MonoBehaviour
{
    #region 상태머신 및 선언
    public EnemyStateMachine StateMachine { get; private set; }
    public EnemyIdleState IdleState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }
    #endregion
    public EnemyDataSO enemyData;
    [Header("스탯")]
    [HideInInspector] public float maxHP;
    [HideInInspector] public float moveSpeed ;
    [HideInInspector] public float detectRange;
    [HideInInspector] public float attackRange;
    [HideInInspector] public float damage;
    private float _currentHP;

    public Rigidbody _rb { get; private set; }
    public Transform _target { get; private set; }

    public Animator _animator { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        StateMachine = new EnemyStateMachine();
        IdleState = new EnemyIdleState(this, StateMachine);
        ChaseState = new EnemyChaseState(this, StateMachine);
        AttackState = new EnemyAttackState(this, StateMachine);
        if(enemyData != null) 
        {
            maxHP = enemyData.maxHP;
            moveSpeed = enemyData.moveSpeed;
            detectRange = enemyData.detectRange;
            attackRange = enemyData.attackRange;
            damage = enemyData.damage;

        }
    }
    private void OnEnable()
    {
        if(_animator != null)
        {
            _animator.Rebind();
            _animator.Update(0f);
        }
        _currentHP = maxHP;
        if(_rb != null)
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        if(PlayerController.Instance != null)
        {
            _target = PlayerController.Instance.transform;
        }
        StateMachine.Initialize(IdleState);
        GetComponent<Collider>().enabled = true;
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
        Debug.Log($"으악! 남은체력 : {_currentHP}");
        if(_currentHP > 0 && _animator != null)
        {
            _animator.SetTrigger("Hit");
        }
        StartCoroutine(FlashRed());
        if (_currentHP <= 0) OnDead();
        
    }
    System.Collections.IEnumerator FlashRed()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if(sr != null)
        {
            Color originColor = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = originColor;
        }
    }
    public void OnDead()
    {
        if (GameManager.Instance != null) GameManager.Instance.AddkillCount();
        if (DataManager.instance != null)
        {
            int reward = (enemyData != null) ? enemyData.goldReward : 10;
            DataManager.instance.AddGold(reward);
        }
        if (GameManager.Instance != null && GameManager.Instance.expGemPrefab != null)
        {
            GameObject gemobj = Instantiate(GameManager.Instance.expGemPrefab, transform.position, Quaternion.identity);
            ExpGem gemScript = gemobj.GetComponent<ExpGem>();
            if (gemScript != null && PlayerController.Instance != null)
            {
                gemScript.Initialize(PlayerController.Instance.transform);
            }
        }
        StartCoroutine(CoDead());
    }
    IEnumerator CoDead()
    {
        GetComponent<Collider>().enabled = false;
        _rb.velocity = Vector3.zero;
        if (_animator != null) _animator.SetTrigger("Dead");
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }
}
