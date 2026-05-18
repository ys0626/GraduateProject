using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// MCTS의 3. Simulation 단계
/// </summary>
public static class MCTSSimulation
{
    /// <summary>
    /// 현재 상태에서 랜덤 시뮬레이션 수행
    /// </summary>
    public static float Simulate(
        SimGameState state,
        int maxDepth = 50)
    {
        // =================================================
        // 원본 상태 복사
        // =================================================

        SimGameState sim =
            state.Clone();

        // =================================================
        // 랜덤 플레이아웃
        // =================================================

        for (int depth = 0;
             depth < maxDepth;
             depth++)
        {
            // =============================================
            // 종료 체크
            // =============================================

            if (sim.self.CurrentHP <= 0)
            {
                return -0.8f;
            }

            if (sim.opponent.CurrentHP <= 0)
            {
                return +1f;
            }

            // =============================================
            // 현재 턴 Entity
            // =============================================

            SimEntity current =
                sim.selfTurn
                ? sim.self
                : sim.opponent;

            SimEntity target =
                sim.selfTurn
                ? sim.opponent
                : sim.self;

            // =============================================
            // 사용 가능한 카드 찾기
            // =============================================

            List<CardInstance> playableCards =
                current.hand.FindAll(
                    c => c.currentCost <=
                         current.CurrentEnergy);

            // =============================================
            // 플레이 가능한 카드 없음
            // =============================================

            if (playableCards.Count == 0)
            {
                EndTurn(sim);

                continue;
            }

            // =============================================
            // 랜덤 카드 선택
            // =============================================

            CardInstance selectedCard =
                playableCards[
                    Random.Range(
                        0,
                        playableCards.Count)];

            // =============================================
            // 카드 사용
            // =============================================

            SimBattleHelper.TryUseCard(
                current,
                target,
                selectedCard);

            // =============================================
            // 카드 사용 후
            // 플레이 가능한 카드가 더 있는지 확인
            // =============================================

            bool hasPlayableCard =
                current.hand.Exists(
                    c => c.currentCost <=
                         current.CurrentEnergy);

            // 더 이상 플레이 불가능하면 턴 종료
            if (!hasPlayableCard)
            {
                EndTurn(sim);
            }
        }

        // =================================================
        // 최대 depth 도달 시 평가
        // =================================================

        return Evaluate(sim);
    }



    /// <summary>
    /// 턴 종료 + 다음 턴 시작 처리
    /// </summary>
    private static void EndTurn(
        SimGameState sim)
    {
        // =================================================
        // 현재 턴 플레이어
        // =================================================

        SimEntity current =
            sim.selfTurn
            ? sim.self
            : sim.opponent;

        // =================================================
        // 턴 종료 처리
        // =================================================

        // 남은 에너지 제거
        current.CurrentEnergy = 0;

        // 손패 버리기
        current.DiscardHand();

        // 상태 감소
        current.TickStatuses();

        // =================================================
        // 턴 전환
        // =================================================

        sim.selfTurn = !sim.selfTurn;

        sim.turnCount++;

        // =================================================
        // 다음 턴 플레이어
        // =================================================

        SimEntity next =
            sim.selfTurn
            ? sim.self
            : sim.opponent;

        // =================================================
        // 턴 시작 처리
        // =================================================

        // Block 제거
        next.Block = 0;

        // 에너지 회복
        next.CurrentEnergy =
            next.MaxEnergy;

        // 카드 드로우
        next.DrawCards(5);
    }


    /// <summary>
    /// 평가 함수
    /// </summary>
    public static float Evaluate(SimGameState state)
    {
        if (state.self.CurrentHP <= 0)
            return -1f;

        if (state.opponent.CurrentHP <= 0)
            return 1f;
        
        float score = (state.self.CurrentHP - state.opponent.CurrentHP) * 0.03f;
        score += (state.self.Block - state.opponent.Block) * 0.01f;
        score += state.self.GetStatusValue(StatusType.Strength) * 0.1f;
        score += state.self.GetStatusValue(StatusType.Dexterity) * 0.1f;
        score += state.opponent.GetStatusValue(StatusType.Weak) * 0.05f;
        score += state.opponent.GetStatusValue(StatusType.Vulnerable) * 0.05f;

        return Tanh(score);
    }

    private static float Tanh(float x)
    {
        float ePos = Mathf.Exp(x);
        float eNeg = Mathf.Exp(-x);
        return (ePos - eNeg) / (ePos + eNeg);
    }
}