using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 깊이 제한 Minimax 알고리즘
/// </summary>
public class MinimaxController : IEntityController
{
    // =====================================================
    // 설정값
    // =====================================================

    private const int SEARCH_DEPTH = 3;

    // =====================================================
    // 외부 호출
    // =====================================================

    public CardInstance SelectCard(Entity entity)
    {
        // =================================================
        // 루트 상태 생성
        // =================================================

        SimGameState rootState =
            SimGameState.Create(entity);

        // =================================================
        // 실제 게임 기준 사용 가능한 카드
        // =================================================

        List<CardInstance> playableCards =
            GetPlayableCards(entity);

        // 플레이 가능한 카드 없음
        if (playableCards.Count == 0)
        {
            return null;
        }

        // =================================================
        // 최적 카드 탐색
        // =================================================

        CardInstance bestCard = null;

        float bestValue =
            float.NegativeInfinity;

        foreach (CardInstance card in playableCards)
        {
            // 상태 복사
            SimGameState next =
                rootState.Clone();

            // clone 내부 카드 찾기
            CardInstance simCard =
                FindCardByID(
                    next.self,
                    card.instanceID);

            if (simCard == null)
            {
                continue;
            }

            // 카드 사용
            SimBattleHelper.TryUseCard(
                next.self,
                next.opponent,
                simCard);

            // 추가 행동 가능 여부
            bool hasPlayableCard =
                HasPlayableCard(next.self);

            // 행동 불가능하면 턴 종료
            if (!hasPlayableCard)
            {
                EndTurn(next);
            }

            // minimax 탐색
            float value =
                Minimax(
                    next,
                    SEARCH_DEPTH - 1);

            // 최고 행동 갱신
            if (value > bestValue)
            {
                bestValue = value;
                bestCard = card;
            }
        }

        return bestCard;
    }

    // =====================================================
    // Minimax
    // =====================================================

    private float Minimax(
        SimGameState state,
        int depth)
    {
        // =================================================
        // 종료 조건
        // =================================================

        if (depth <= 0 ||
            state.self.CurrentHP <= 0 ||
            state.opponent.CurrentHP <= 0)
        {
            return Evaluate(state);
        }

        // =================================================
        // 현재 플레이어
        // =================================================

        SimEntity current =
            state.selfTurn
            ? state.self
            : state.opponent;

        // =================================================
        // 사용 가능한 카드
        // =================================================

        List<CardInstance> playableCards =
            GetPlayableCards(current);

        // =================================================
        // 행동 불가능 → 턴 종료
        // =================================================

        if (playableCards.Count == 0)
        {
            SimGameState next =
                state.Clone();

            EndTurn(next);

            return Minimax(
                next,
                depth - 1);
        }

        // =================================================
        // MAX
        // =================================================

        if (state.selfTurn)
        {
            float best =
                float.NegativeInfinity;

            foreach (CardInstance card in playableCards)
            {
                SimGameState next =
                    state.Clone();

                SimEntity currentClone =
                    next.selfTurn
                    ? next.self
                    : next.opponent;

                SimEntity targetClone =
                    next.selfTurn
                    ? next.opponent
                    : next.self;

                CardInstance simCard =
                    FindCardByID(
                        currentClone,
                        card.instanceID);

                if (simCard == null)
                {
                    continue;
                }

                // 카드 사용
                SimBattleHelper.TryUseCard(
                    currentClone,
                    targetClone,
                    simCard);

                // 추가 행동 가능 여부
                bool hasPlayableCard =
                    HasPlayableCard(currentClone);

                // 행동 종료 처리
                if (!hasPlayableCard)
                {
                    EndTurn(next);
                }

                float value =
                    Minimax(
                        next,
                        depth - 1);

                best =
                    Mathf.Max(best, value);
            }

            return best;
        }

        // =================================================
        // MIN
        // =================================================

        else
        {
            float best =
                float.PositiveInfinity;

            foreach (CardInstance card in playableCards)
            {
                SimGameState next =
                    state.Clone();

                SimEntity currentClone =
                    next.selfTurn
                    ? next.self
                    : next.opponent;

                SimEntity targetClone =
                    next.selfTurn
                    ? next.opponent
                    : next.self;

                CardInstance simCard =
                    FindCardByID(
                        currentClone,
                        card.instanceID);

                if (simCard == null)
                {
                    continue;
                }

                // 카드 사용
                SimBattleHelper.TryUseCard(
                    currentClone,
                    targetClone,
                    simCard);

                // 추가 행동 가능 여부
                bool hasPlayableCard =
                    HasPlayableCard(currentClone);

                // 행동 종료 처리
                if (!hasPlayableCard)
                {
                    EndTurn(next);
                }

                float value =
                    Minimax(
                        next,
                        depth - 1);

                best =
                    Mathf.Min(best, value);
            }

            return best;
        }
    }

    // =====================================================
    // 평가 함수
    // =====================================================

    private float Evaluate(SimGameState state)
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

    // =====================================================
    // 사용 가능한 카드 수집 (SimEntity)
    // =====================================================

    private List<CardInstance> GetPlayableCards(
        SimEntity entity)
    {
        List<CardInstance> result =
            new List<CardInstance>();

        foreach (CardInstance card in entity.hand)
        {
            if (card.currentCost
                <= entity.CurrentEnergy)
            {
                result.Add(card);
            }
        }

        return result;
    }

    // =====================================================
    // 사용 가능한 카드 수집 (실제 Entity)
    // =====================================================

    private List<CardInstance> GetPlayableCards(
        Entity entity)
    {
        List<CardInstance> result =
            new List<CardInstance>();

        foreach (CardInstance card in entity.hand)
        {
            if (card.currentCost
                <= entity.CurrentEnergy)
            {
                result.Add(card);
            }
        }

        return result;
    }

    // =====================================================
    // 플레이 가능한 카드 존재 여부
    // =====================================================

    private bool HasPlayableCard(
        SimEntity entity)
    {
        foreach (CardInstance card in entity.hand)
        {
            if (card.currentCost
                <= entity.CurrentEnergy)
            {
                return true;
            }
        }

        return false;
    }

    // =====================================================
    // instanceID 기반 카드 탐색
    // =====================================================

    private CardInstance FindCardByID(
        SimEntity entity,
        int instanceID)
    {
        foreach (CardInstance card in entity.hand)
        {
            if (card.instanceID == instanceID)
            {
                return card;
            }
        }

        return null;
    }

    // =====================================================
    // 턴 종료 처리
    // =====================================================

    private void EndTurn(
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

        // 에너지 제거
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
        // 다음 플레이어
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

    // =====================================================
    // Tanh 정규화
    // =====================================================

    private float Tanh(float x)
    {
        float ePos = Mathf.Exp(x);
        float eNeg = Mathf.Exp(-x);

        return (ePos - eNeg)
            / (ePos + eNeg);
    }
}