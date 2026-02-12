using Autodesk.Fbx;
using DG.Tweening;
using System.Collections;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
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

    private float _currentHP;
    private GameObject _originalPrefab;
    private static readonly int HashHit = Animator.StringToHash("Hit");
    private static readonly int HashDead = Animator.StringToHash("Dead");

    private bool isKnockback = false;
    private bool _isDead = false; 

    public float MoveSpeed => enemyData != null ? enemyData.moveSpeed : 3f;
    public float AttackRange => enemyData != null ? enemyData.attackRange : 1f;
    public float DetectRange => enemyData != null ? enemyData.detectRange : 10f;
    public float Damage => enemyData != null ? enemyData.damage : 10f;
    public float Defense => enemyData != null ? enemyData.defense : 0f;

    public GameObject damagePopupPrefab;
    private void OnEnable()
    {
        if(PlayerController.Instance != null)
        {
            PlayerController.Instance.OnPlayerDie += HandlePlayerDie;
        }
    }
    private void OnDisable()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnPlayerDie -= HandlePlayerDie;
        }
    }
    void HandlePlayerDie()
    {
        enabled = false;
    }
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

        isKnockback = false;
        _isDead = false;

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
        if (isKnockback) return;
        StateMachine.currentState.PhysicsUpdate();
    }
    public void KnockBack(Vector3 dir, float force)
    {
        if (isKnockback || _isDead) return;
        StartCoroutine(CoKnockBack(dir, force));
    }
    private IEnumerator CoKnockBack(Vector3 dir, float force)
    {
        isKnockback = true;
        _rb.velocity = Vector3.zero;
        _rb.AddForce(dir * force, ForceMode.Impulse);

        yield return new WaitForSeconds(0.2f);

        _rb.velocity = Vector3.zero;
        isKnockback = false;
    }
    public void TakeDamage(float damage)
    {
        float finalDamage = Mathf.Max(1, damage - Defense);
        _currentHP -= finalDamage;


        //데미지 팝업 로직
        if(damagePopupPrefab != null && PoolManager.Instance != null)
        {
            // 1. 풀에서 가져오기 (위치 내 머리 위 + 랜덤성 약간)
            GameObject popUp = PoolManager.Instance.Get(damagePopupPrefab);

            // 위치 설정: 적의 위쪽 + 약간의 랜덤 좌우 (겹침 방지)
            float randomX = Random.Range(-0.3f, 0.3f);
            popUp.transform.position = transform.position + Vector3.up * 1.5f + new Vector3(randomX, 0, 0);

            // 2. 텍스트 세팅 (데미지, 프리팹 정보 전달)
            // 크리티컬 여부는 현재 함수 인자에 없으므로 디폴트 false로 처리
            DamagePopup popuScript = popUp.GetComponent<DamagePopup>();
            if (popuScript != null) popuScript.Setup(finalDamage, damagePopupPrefab, false);
        }
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
        if (DataManager.Instance != null && enemyData != null) DataManager.Instance.AddStageGold(enemyData.goldReward);

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

        DOVirtual.DelayedCall(0.5f, () => ReturnToPool());
    }
    private void ReturnToPool()
    {
        _isDead = false;
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Return(gameObject, _originalPrefab);
        }
        else
        {
            Destroy(gameObject);
        }
        transform.DOKill();
    }

}
