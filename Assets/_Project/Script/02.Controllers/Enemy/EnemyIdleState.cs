using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    public EnemyIdleState(EnemyController enemy, EnemyStateMachine stateMachine) : base (enemy, stateMachine) { }

    public override void Enter()
    {
        base.Enter();
        enemy._rb.velocity = Vector3.zero;
        if (enemy._animator != null) enemy._animator.SetBool("isMoving", false);
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (enemy._target == null) return;
        float distance = Vector3.Distance (enemy.transform.position, enemy._target.position);

        if(distance < enemy.DetectRange)
        {
            stateMachine.ChangeState(enemy.ChaseState);
        }
    }
}
