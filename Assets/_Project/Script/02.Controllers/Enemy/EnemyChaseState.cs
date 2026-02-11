using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    public EnemyChaseState(EnemyController enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if(enemy._target == null) 
        {
            stateMachine.ChangeState(enemy.IdleState);
            return;
        }
        float distance = Vector3.Distance(enemy.transform.position, enemy._target.position);

        if(enemy._animator != null) enemy._animator.SetBool("isMoving", true);
        if (distance > enemy.DetectRange *2f)
        {
            stateMachine.ChangeState(enemy.IdleState);
        }
        if(distance <= enemy.AttackRange)
        {
            stateMachine.ChangeState(enemy.AttackState);
        }
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if (enemy._target == null) return;
        Vector3 dir = (enemy._target.position - enemy.transform.position).normalized;
        enemy._rb.velocity = dir * enemy.MoveSpeed;
        if (enemy._animator != null)
        {
            Transform visual = enemy._animator.transform;
            float size = Mathf.Abs(visual.localScale.x);
            if (dir.x > 0)
            {
                visual.localScale = new Vector3(-size, size, size);
                //enemy.transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (dir.x < 0)
            {
                visual.localScale = new Vector3(size, size, size);
                //enemy.transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }
}
