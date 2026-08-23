using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// MCTS의 3. Simulation 단계
/// </summary>
public static class MCTSSimulation
{
    // =====================================================
    // 진입점 1: 노드 기준 (캐싱 + 조기종료 + 이력 이어받기)
    // =====================================================

    public static float Simulate(MCTSNode node, int maxDepth = 20)
    {
        ulong hash = node.state.GetStateHash();

        if (MCTSTranspositionTable.TryGet(hash, out float cached))
        {
            return cached;
        }

        if (node.ShouldSkipSimulation())
        {
            MCTSTranspositionTable.Store(hash, node.AverageReward);
            return node.AverageReward;
        }

        float result = Simulate(node.state, node.turnHistory, maxDepth);

        MCTSTranspositionTable.Store(hash, result);

        return result;
    }

    // =====================================================
    // 진입점 2: 상태만 있을 때 (이력 없이 새로 시작)
    // =====================================================

    public static float Simulate(SimGameState state, int maxDepth = 20)
    {
        return Simulate(state, default, maxDepth);
    }

    // =====================================================
    // 실제 롤아웃 로직 (이력 추적 포함)
    // =====================================================

    private static float Simulate(
        SimGameState state,
        TurnPlayHistory initialHistory,
        int maxDepth)
    {
        // =================================================
        // 원본 상태 복사
        // =================================================

        SimGameState sim = state.Clone();

        TurnPlayHistory history = initialHistory;

        // =================================================
        // 랜덤 플레이아웃
        // =================================================

        for (int depth = 0; depth < maxDepth; depth++)
        {
            // =============================================
            // 종료 체크
            // =============================================

            if (sim.self.CurrentHP <= 0 || sim.opponent.CurrentHP <= 0)
            {
                return Evaluate(sim);
            }

            // =============================================
            // 현재 턴 Entity
            // =============================================

            SimEntity current = sim.selfTurn ? sim.self : sim.opponent;
            SimEntity target = sim.selfTurn ? sim.opponent : sim.self;

            // =============================================
            // 사용 가능한 카드 찾기 (휴리스틱 필터 적용)
            // =============================================

            List<CardInstance> playableCards =
                GetHeuristicPlayableCards(current, history);

            // =============================================
            // 플레이 가능한 카드 없음
            // =============================================

            if (playableCards.Count == 0)
            {
                EndTurn(sim);
                history = default;
                continue;
            }

            // =============================================
            // 롤아웃 정책에 따라 카드 선택
            // =============================================

            CardInstance selectedCard =
                SelectWeightedCard(playableCards, current, target, sim);

            // =============================================
            // 카드 사용
            // =============================================

            SimBattleHelper.TryUseCard(current, target, selectedCard);

            history = history.Extend(selectedCard.data);

            // =============================================
            // 카드 사용 후
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
    // 4가지 휴리스틱 규칙 적용된 플레이 가능 카드 필터
    // =====================================================

    private static List<CardInstance> GetHeuristicPlayableCards(
        SimEntity current,
        TurnPlayHistory history)
    {
        List<CardInstance> result = new List<CardInstance>();

        foreach (CardInstance card in current.hand)
        {
            if (card.currentCost > current.CurrentEnergy)
                continue;

            CardData data = card.data;

            // 규칙 4: DoubleTap 이후엔 공격 카드만
            if (history.hasDoubleTap && data.cardType != CardType.Attack)
                continue;

            // 규칙 2-a: 순수공격 이후 디버프 카드 배제
            if (history.hasPlainAttack && (data.tags & CardTag.Debuff) != 0)
                continue;

            // 규칙 2-b, 3: 공격/한계돌파 이후 힘 증가 카드 배제
            if ((history.hasAnyAttack || history.hasLimitBreak) &&
                (data.tags & CardTag.StrengthGain) != 0)
                continue;

            // 규칙 4: 후속 공격 카드 없으면 DoubleTap 자체 배제
            if ((data.tags & CardTag.DoubleTap) != 0 &&
                !HasFollowUpAttack(current, card))
                continue;

            result.Add(card);
        }

        return result;
    }

    private static bool HasFollowUpAttack(SimEntity current, CardInstance doubleTapCard)
    {
        int remainingEnergy = current.CurrentEnergy - doubleTapCard.currentCost;

        foreach (var other in current.hand)
        {
            if (other == doubleTapCard) continue;

            if (other.data.cardType == CardType.Attack &&
                other.currentCost <= remainingEnergy)
                return true;
        }

        return false;
    }

    // =====================================================
    // 턴 종료 + 다음 턴 시작 처리
    // =====================================================

    private static void EndTurn(SimGameState sim)
    {
        SimEntity current = sim.selfTurn ? sim.self : sim.opponent;

        // 남은 에너지 제거
        current.CurrentEnergy = 0;

        // 손패 버리기
        current.DiscardHand();

        // 상태 감소
        current.TickStatuses();

        // 턴 전환
        sim.selfTurn = !sim.selfTurn;
        sim.turnCount++;

        SimEntity next = sim.selfTurn ? sim.self : sim.opponent;

        // Block 제거
        next.Block = 0;

        // 에너지 회복
        next.CurrentEnergy = next.MaxEnergy;

        // 카드 드로우
        next.DrawCards(5);
    }

    // =====================================================
    // 평가 함수
    // =====================================================

    private static float Evaluate(SimGameState state)
    {
        // 승리
        if (state.opponent.CurrentHP <= 0) return 0.05f;

        // 패배
        if (state.self.CurrentHP <= 0) return -0.05f;

        float score = 0f;

        // HP 차이
        score += (state.self.CurrentHP - state.opponent.CurrentHP) * 0.01f;

        // Block 차이
        score += (state.self.Block - state.opponent.Block) * 0.008f;

        // 상태 이상 가치
        score += state.self.GetStatusValue(StatusType.Strength) * 0.05f;
        score += state.self.GetStatusValue(StatusType.Dexterity) * 0.05f;

        score += state.opponent.GetStatusValue(StatusType.Weak) * 0.01f;
        score += state.opponent.GetStatusValue(StatusType.Vulnerable) * 0.01f;

        return Tanh(score);
    }

    private static float Tanh(float x)
    {
        float ePos = Mathf.Exp(x);
        float eNeg = Mathf.Exp(-x);
        return (ePos - eNeg) / (ePos + eNeg);
    }

    // =====================================================
    // heuristic 기반 weighted random 선택
    // =====================================================

    private static CardInstance SelectWeightedCard(
        List<CardInstance> playableCards,
        SimEntity self,
        SimEntity target,
        SimGameState sim)
    {
        List<float> scores = new List<float>();
        float totalScore = 0f;

        foreach (CardInstance card in playableCards)
        {
            float score = GetCardScore(card, self, target, sim);

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

        // fallback
        return playableCards[0];
    }

    // =====================================================
    // 카드 heuristic score
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