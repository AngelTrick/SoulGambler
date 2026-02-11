public abstract class EnemyBaseState 
{
    protected EnemyStateMachine stateMachine;
    protected EnemyController enemy;

    public EnemyBaseState(EnemyController enemy, EnemyStateMachine stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }
    public virtual void Enter() { }
    public virtual void LogicUpdate() { }
    public virtual void PhysicsUpdate() { }
    public virtual void Exit() { }
}
