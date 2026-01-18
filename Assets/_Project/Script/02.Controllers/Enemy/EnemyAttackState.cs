using DG.Tweening.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    private float _attackTimer;
    private float _attackCoolDown = 1.5f;

    public EnemyAttackState(EnemyController enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        enemy._rb.velocity = Vector3.zero;
        _attackTimer = 0f;
        if(enemy._animator != null) 
        {
            enemy._animator.SetBool("isMoving", false);
        }
        LookAtTarget();
    }
    private void LookAtTarget()
    {
        if (enemy._target == null || enemy._animator == null) return;
        Vector3 dir = (enemy._target.position - enemy.transform.position).normalized;
        enemy._rb.velocity = Vector3.zero;
        if (enemy._animator != null)
        {
            Transform visual = enemy._animator.transform;
            float size = Mathf.Abs(visual.localScale.x);
            if (dir.x > 0)
            {
                visual.localScale = new Vector3(-size, size, size);
                //enemy.transform.localScale = new Vector3(1, 1, 1);
            }
            else if (dir.x < 0)
            {
                visual.localScale = new Vector3(size, size, size);
                //enemy.transform.localScale = new Vector3(-1, 1, 1);
            }
        }

    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (enemy._target == null) return;
        LookAtTarget();
        float distance = Vector3.Distance(enemy.transform.position, enemy._target.position);
        if(distance > enemy.AttackRange + 0.5f)
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }
        _attackTimer += Time.deltaTime;
        if(_attackTimer >= _attackCoolDown)
        {
            Attack();
            _attackTimer = 0f;
        }
    }
    private void Attack()
    {
        if (enemy._target == null) return;
        Debug.Log("적 공격 시도");
        PlayerController player = enemy._target.GetComponent<PlayerController>();

        if (player != null)
        {
            player.TakeDamage(enemy.Damage);
            Debug.Log("플레이어에게 데미지 전달 성공");
        }
        else
        {
            Debug.Log("해당 플레이어의 스크립트를 찾지 못했습니다.");
        }
        if (enemy._animator != null) enemy._animator.SetTrigger("Attack");
    }
    
}
