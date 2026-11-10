
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MCTS의 3. Simulation 단계
/// </summary>
public static class MCTSSimulation
{
    // =====================================================
    // 평가 함수 설정값
    // =====================================================

    // 기존 평가
    private const float LEGACY_WIN_REWARD = 0.05f;
    private const float LEGACY_LOSE_REWARD = -0.05f;
    private const float LEGACY_EXPLORATION_CONSTANT = 1f;

    // 개선 평가
    private const float IMPROVED_WIN_REWARD = 1f;
    private const float IMPROVED_LOSE_REWARD = -1f;
    private const float IMPROVED_NON_TERMINAL_SCALE = 0.9f;
    private const float IMPROVED_EXPLORATION_CONSTANT = 2.82842712f;

    /// <summary>
    /// 개선 평가 사용 여부
    /// </summary>
    private static bool IsImprovedEvaluationOn()
    {
        return MCTSSettings.EnableImprovedEvaluation;
    }

    /// <summary>
    /// UCT 탐험 상수
    /// </summary>
    public static float GetExplorationConstant()
    {
        return IsImprovedEvaluationOn()
            ? IMPROVED_EXPLORATION_CONSTANT
            : LEGACY_EXPLORATION_CONSTANT;
    }

    // =====================================================
    // 측정용 카운터
    // =====================================================

    public static int RolloutCount;
    public static int SimulationSkipCount;

    // =====================================================
    // 진입점 1: 노드 기준
    // 캐싱 + 시뮬레이션 생략 + 이력 이어받기
    // =====================================================

    public static float Simulate(MCTSNode node, int maxDepth = 20)
    {
        bool cachingOn = MCTSSettings.EnableTranspositionCache;

        ulong hash = default;

        if (cachingOn)
        {
            hash = node.state.GetStateHash(node.turnHistory);

            if (MCTSTranspositionTable.TryGet(hash, out float cached))
                return cached;
        }

        if (node.ShouldSkipSimulation())
        {
            SimulationSkipCount++;

            float avg = node.AverageReward;

            bool skippedValueStoreOn =
                MCTSSettings.EnableSkippedValueStore;

            if (cachingOn && skippedValueStoreOn)
            {
                MCTSTranspositionTable.Store(hash, avg);
            }

            return avg;
        }

        float result = Simulate(
            node.state,
            node.turnHistory,
            maxDepth
        );

        if (cachingOn)
        {
            MCTSTranspositionTable.Store(hash, result);
        }

        return result;
    }

    // =====================================================
    // 진입점 2: 상태만 있을 때
    // =====================================================

    public static float Simulate(SimGameState state, int maxDepth = 20)
    {
        return Simulate(state, default, maxDepth);
    }

    // =====================================================
    // 실제 롤아웃 로직
    // =====================================================

    private static float Simulate(
        SimGameState state,
        TurnPlayHistory initialHistory,
        int maxDepth)
    {
        RolloutCount++;

        // 원본 상태 복사
        SimGameState sim = state.Clone();

        TurnPlayHistory history = initialHistory;

        // =================================================
        // 랜덤 플레이아웃
        // =================================================

        for (int depth = 0; depth < maxDepth; depth++)
        {
            // =============================================
            // 1. 종료 체크
            // =============================================

            if (sim.self.CurrentHP <= 0 ||
                sim.opponent.CurrentHP <= 0)
            {
                return Evaluate(sim);
            }

            // =============================================
            // 2. 현재 턴 Entity
            // =============================================

            SimEntity current = sim.selfTurn
                ? sim.self
                : sim.opponent;

            SimEntity target = sim.selfTurn
                ? sim.opponent
                : sim.self;

            // =============================================
            // 3. 사용 가능한 카드 찾기
            // =============================================

            List<CardInstance> playableCards =
                GetHeuristicPlayableCards(current, history);

            // =============================================
            // 4. 플레이 가능한 카드 없음
            // =============================================

            if (playableCards.Count == 0)
            {
                EndTurn(sim);
                history = default;

                continue;
            }

            // =============================================
            // 5. 롤아웃 정책에 따라 카드 선택
            // =============================================

            CardInstance selectedCard =
                SelectWeightedCard(
                    playableCards,
                    current,
                    target,
                    sim
                );

            // =============================================
            // 6. 카드 사용
            // =============================================

            bool success = SimBattleHelper.TryUseCard(
                current,
                target,
                selectedCard
            );

            if (!success)
            {
                // 카드 사용 실패 시 현재 턴 종료
                EndTurn(sim);
                history = default;

                continue;
            }

            // 정상적으로 사용한 카드만 이력에 기록
            history = history.Extend(selectedCard.data);

            // =============================================
            // 7. 카드 사용 후
            // 플레이 가능한 카드가 더 있는지 확인
            // =============================================

            bool hasPlayableCard =
                GetHeuristicPlayableCards(current, history).Count > 0;

            if (!hasPlayableCard)
            {
                EndTurn(sim);
                history = default;
            }
        }

        // =================================================
        // 최대 depth 도달 시 평가
        // =================================================

        return Evaluate(sim);
    }

    // =====================================================
    // 플레이 가능 카드 필터
    // =====================================================

    private static List<CardInstance> GetHeuristicPlayableCards(
        SimEntity current,
        TurnPlayHistory history)
    {
        bool pruningOn = MCTSSettings.EnableHeuristicRollout;

        List<CardInstance> result = new List<CardInstance>();

        foreach (CardInstance card in current.hand)
        {
            if (card.currentCost > current.CurrentEnergy)
                continue;

            if (!pruningOn)
            {
                result.Add(card);
                continue;
            }

            CardData data = card.data;

            if (history.hasDoubleTap &&
                data.cardType != CardType.Attack)
            {
                continue;
            }

            if (history.hasPlainAttack &&
                (data.tags & CardTag.Debuff) != 0)
            {
                continue;
            }

            if ((history.hasAnyAttack || history.hasLimitBreak) &&
                (data.tags & CardTag.StrengthGain) != 0)
            {
                continue;
            }

            if ((data.tags & CardTag.DoubleTap) != 0 &&
                !HasFollowUpAttack(current, card))
            {
                continue;
            }

            result.Add(card);
        }

        return result;
    }

    private static bool HasFollowUpAttack(
        SimEntity current,
        CardInstance doubleTapCard)
    {
        int remainingEnergy =
            current.CurrentEnergy - doubleTapCard.currentCost;

        foreach (CardInstance other in current.hand)
        {
            if (other == doubleTapCard)
                continue;

            if (other.data.cardType == CardType.Attack &&
                other.currentCost <= remainingEnergy)
            {
                return true;
            }
        }

        return false;
    }

    // =====================================================
    // 턴 종료 + 다음 턴 시작 처리
    // 현재 구조: self = 적, opponent = 플레이어
    // =====================================================

    private static void EndTurn(SimGameState sim)
    {
        // 현재 턴 Entity
        SimEntity current = sim.selfTurn
            ? sim.self
            : sim.opponent;

        // =============================================
        // 1. 남은 에너지 제거
        // =============================================

        current.CurrentEnergy = 0;

        // =============================================
        // 2. 손패 버리기
        // Ethereal 카드는 SimEntity.DiscardHand()에서
        // 소멸 처리해야 함
        // =============================================

        current.DiscardHand();

        // =============================================
        // 3. 턴 종료 상태 효과 처리
        // =============================================

        current.TickStatuses();

        // =============================================
        // 4. 턴 전환
        // =============================================

        // 현재 selfTurn이 true라면 적 턴이 끝난 것
        bool enemyTurnEnded = sim.selfTurn;

        sim.selfTurn = !sim.selfTurn;

        // 적 턴 종료 후 플레이어 턴이 시작될 때만 증가
        if (enemyTurnEnded)
        {
            sim.turnCount++;
        }

        // 다음 턴 Entity
        SimEntity next = sim.selfTurn
            ? sim.self
            : sim.opponent;

        // =============================================
        // 5. 다음 턴 시작 처리
        // =============================================

        if (sim.selfTurn)
        {
            // 적 턴 시작
            next.Block = 0;
        }
        else
        {
            // 플레이어 턴 시작

            // 방어도 유지 버프가 없다면 방어도 초기화
            if (next.GetStatusValue(StatusType.BlockRetention) <= 0)
            {
                next.Block = 0;
            }

            // 턴 시작 시 힘 증가 버프 적용
            int strengthPerTurn =
                next.GetStatusValue(StatusType.StrengthPerTurn);

            if (strengthPerTurn > 0)
            {
                next.AddStatus(
                    new StrengthStatus(strengthPerTurn)
                );
            }
        }

        // =============================================
        // 6. 에너지 회복
        // =============================================

        next.CurrentEnergy = next.MaxEnergy;

        // =============================================
        // 7. 카드 드로우
        // =============================================

        next.DrawCards(5);
    }

    // =====================================================
    // 평가 함수
    // =====================================================

    private static float Evaluate(SimGameState state)
    {
        bool improvedOn = IsImprovedEvaluationOn();

        // 승리
        if (state.opponent.CurrentHP <= 0)
        {
            return improvedOn
                ? IMPROVED_WIN_REWARD
                : LEGACY_WIN_REWARD;
        }

        // 패배
        if (state.self.CurrentHP <= 0)
        {
            return improvedOn
                ? IMPROVED_LOSE_REWARD
                : LEGACY_LOSE_REWARD;
        }

        float score = 0f;

        // HP 차이
        score +=
            (state.self.CurrentHP - state.opponent.CurrentHP) * 0.01f;

        // Block 차이
        score +=
            (state.self.Block - state.opponent.Block) * 0.008f;

        // 내 상태 이상 가치
        score +=
            state.self.GetStatusValue(StatusType.Strength) * 0.05f;

        score +=
            state.self.GetStatusValue(StatusType.Dexterity) * 0.05f;

        // 상대 상태 이상 가치
        score +=
            state.opponent.GetStatusValue(StatusType.Weak) * 0.01f;

        score +=
            state.opponent.GetStatusValue(StatusType.Vulnerable) * 0.01f;

        // 개선 평가
        if (improvedOn)
        {
            return Tanh(score) * IMPROVED_NON_TERMINAL_SCALE;
        }

        return Tanh(score);
    }

    private static float Tanh(float x)
    {
        float ePos = Mathf.Exp(x);
        float eNeg = Mathf.Exp(-x);

        return (ePos - eNeg) / (ePos + eNeg);
    }

    // =====================================================
    // Heuristic 기반 Weighted Random 선택
    // =====================================================

    private static CardInstance SelectWeightedCard(
        List<CardInstance> playableCards,
        SimEntity self,
        SimEntity target,
        SimGameState sim)
    {
        bool heuristicOn = MCTSSettings.EnableHeuristicRollout;

        // 휴리스틱 롤아웃 OFF
        // 카드 가중치 없이 균등 무작위
        if (!heuristicOn)
        {
            return playableCards[
                Random.Range(0, playableCards.Count)
            ];
        }

        List<float> scores = new List<float>();
        float totalScore = 0f;

        foreach (CardInstance card in playableCards)
        {
            float score = GetCardScore(
                card,
                self,
                target,
                sim
            );

            score = Mathf.Max(1f, score);

            scores.Add(score);
            totalScore += score;
        }

        float randomValue = Random.Range(0f, totalScore);
        float cumulative = 0f;

        for (int i = 0; i < playableCards.Count; i++)
        {
            cumulative += scores[i];

            if (randomValue <= cumulative)
            {
                return playableCards[i];
            }
        }

        // Fallback
        return playableCards[0];
    }

    // =====================================================
    // 카드 Heuristic Score
    // =====================================================

    private static float GetCardScore(
        CardInstance card,
        SimEntity self,
        SimEntity target,
        SimGameState sim)
    {
        float score = 1f;

        switch (card.data.cardType)
        {
            case CardType.Power:

                score += 100f;
                score += Mathf.Max(0, 20 - sim.turnCount);

                break;
        }

        return score;
    }
}
