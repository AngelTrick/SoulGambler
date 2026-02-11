public class EnemyStateMachine 
{
    //현재 기억 상태 변수
   public EnemyBaseState currentState { get; private set; }

    //초기화
    public void Initialize(EnemyBaseState startingState)
    {
        currentState = startingState;
        currentState.Enter();
    }
    //상태 변환 (Ex Idle -> Chase)
    public void ChangeState(EnemyBaseState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }
}
