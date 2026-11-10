/// <summary>
/// 적 AI(MCTS) 최종 설정
/// testing 브랜치의 기법 실험으로 정한 최종 구성(M2)을 고정값으로 둔다.
/// 실험용 토글과 바닐라 MCTS 비교는 testing 브랜치에서만 사용하며, 값을 바꿀 때는 이 파일만 수정한다.
/// </summary>
public static class MCTSSettings
{
    // =====================================================
    // 탐색
    // =====================================================

    public static readonly bool EnableDynamicSearch = true; // 선택지 수와 HP 차이로 반복 횟수 결정
    public static readonly bool EnableSearchEarlyStop = true; // 1위 child가 2위보다 일정 배수 이상 방문되면 탐색 중단
    public static readonly bool EnableTreeReuse = true; // 같은 턴의 다음 탐색에서 이전 트리 재사용 (Fast Expansion 포함)
    public const int SimulationBudget = 1000; // 최대 반복 횟수 (동적 탐색이 켜져 있으면 목표 반복의 상한)

    // =====================================================
    // 롤아웃 / 확장
    // =====================================================

    public static readonly bool EnableHeuristicRollout = false; // 롤아웃 필터 규칙과 Power 카드 가중치 (끄면 균등 무작위)
    public static readonly bool EnableHeuristicActionPruning = false; // 트리 확장 단계 가지치기
    public static readonly bool EnableLethalCheck = true; // 이번 턴 처치 가능한 카드 순서 우선 확인

    // =====================================================
    // 캐싱
    // =====================================================

    public static readonly bool EnableTranspositionCache = true; // 전위 테이블
    public const int TranspositionReuseCount = 3; // 새 롤아웃 한 번당 저장값 재사용 횟수
    public const int TranspositionMinSamples = 1; // 재사용을 시작하는 최소 표본 수
    public static readonly bool EnableSimulationSkip = false; // 일정 횟수 이상 방문한 노드의 시뮬레이션 생략
    public const int SimulationSkipVisitThreshold = 30; // 시뮬레이션 생략 기준 방문 수
    public static readonly bool EnableSkippedValueStore = false; // 생략 시 노드 평균을 전위 테이블에 저장

    // =====================================================
    // 평가
    // =====================================================

    public static readonly bool EnableImprovedEvaluation = true; // 승 1, 패 -1, 비종결 tanh x 0.9, 탐험 상수 2√2

    // =====================================================
    // 로그 라벨 (자동 시뮬레이션 결과 파일명에 사용, testing 브랜치 라벨과 같은 형식)
    // =====================================================

    public const string ModeLabel = "DS1_HR0_ES1_TT1_RU3_MS1_SK0_SS0_TR1_LC1_AP0_EV1_Budget1000";
}
