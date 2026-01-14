# VamfireSurvivorProject
### 1. Singleton Pattern (싱글톤 패턴)
* **Purpose**: 게임의 전역 상태(점수, 시간)와 시스템(UI, 사운드)을 단일 인스턴스로 관리하고, 어디서든 쉽게 접근하기 위해 사용했습니다.
* **Implementation**: `GameManager`, `UIManager`, `PoolManager`
* **Why**: 씬(Scene) 내에서 유일해야 하는 매니저 클래스들의 중복 생성을 방지하고, 적(Enemy)이나 플레이어 객체 등 다양한 곳에서 매니저 기능에 접근해야 하는 구조적 필요성을 해결했습니다.

### 2. Finite State Machine (FSM / 상태 패턴)
* **Purpose**: 몬스터의 복잡한 AI 로직을 상태별로 모듈화하여 관리했습니다.
* **Implementation**: `EnemyStateMachine`, `EnemyBaseState` (Abstract), `EnemyChaseState`, `EnemyAttackState` , `EnemyIdleState`
* **Why**: 거대한 `if-else` 문을 사용하는 대신, 상태를 클래스로 분리하여 코드 가독성을 높였습니다. 이를 통해 추후 '도망', '스킬 사용' 등의 새로운 행동 패턴이 추가되더라도 기존 코드를 수정하지 않고 확장이 가능합니다.

### 3. Object Pooling (오브젝트 풀링)
* **Purpose**: 다수의 적이 생성되고 파괴되는 뱀서라이크 장르 특성에 맞춰 메모리 할당/해제 비용을 최적화했습니다.
* **Implementation**: `PoolManager`, `EnemySpawner`
* **Why**: 잦은 `Instantiate`와 `Destroy` 호출로 인한 가비지 컬렉션(GC) 스파이크와 프레임 저하를 방지하여, 수백 마리의 적이 등장해도 안정적인 프레임을 유지하도록 설계했습니다.

### 4. Data-Driven Design (데이터 주도 설계)
* **Purpose**: 게임 로직과 밸런스 데이터(수치)를 분리했습니다.
* **Implementation**: `ScriptableObject` (`PlayerDataSO`, `EnemyDataSO`)
* **Why**: 캐릭터의 체력, 공격력, 속도 등의 수치를 코드 내부가 아닌 에셋 파일(.asset)로 관리함으로써, 컴파일 없이 에디터 상에서 실시간으로 밸런스를 조정할 수 있는 환경을 구축했습니다.
