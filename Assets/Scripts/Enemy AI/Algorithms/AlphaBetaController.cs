using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 깊이 제한 Alpha-Beta Pruning 알고리즘
/// </summary>
public class AlphaBetaController : IEntityController
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
        SimGameState rootState =
            SimGameState.Create(entity);

        List<CardInstance> playableCards =
            GetPlayableCards(entity);

        if (playableCards.Count == 0)
        {
            return null;
        }

        CardInstance bestCard = null;

        float bestValue =
            float.NegativeInfinity;

        float alpha = float.NegativeInfinity;
        float beta = float.PositiveInfinity;

        foreach (CardInstance card in playableCards)
        {
            SimGameState next =
                rootState.Clone();

            CardInstance simCard =
                FindCardByID(
                    next.self,
                    card.instanceID);

            if (simCard == null)
                continue;

            SimBattleHelper.TryUseCard(
                next.self,
                next.opponent,
                simCard);

            if (!HasPlayableCard(next.self))
            {
                EndTurn(next);
            }

            float value =
                AlphaBeta(
                    next,
                    SEARCH_DEPTH - 1,
                    alpha,
                    beta,
                    false);

            if (value > bestValue)
            {
                bestValue = value;
                bestCard = card;
            }

            // MAX root에서도 alpha 갱신
            alpha = Mathf.Max(alpha, bestValue);
        }

        return bestCard;
    }

    // =====================================================
    // Alpha-Beta
    // =====================================================

    private float AlphaBeta(
        SimGameState state,
        int depth,
        float alpha,
        float beta,
        bool isMax)
    {
        // 종료 조건
        if (depth <= 0 ||
            state.self.CurrentHP <= 0 ||
            state.opponent.CurrentHP <= 0)
        {
            return Evaluate(state);
        }

        SimEntity current =
            state.selfTurn
            ? state.self
            : state.opponent;

        List<CardInstance> playableCards =
            GetPlayableCards(current);

        // 턴 종료만 가능한 경우
        if (playableCards.Count == 0)
        {
            SimGameState next = state.Clone();
            EndTurn(next);

            return AlphaBeta(
                next,
                depth - 1,
                alpha,
                beta,
                !isMax);
        }

        // =====================================================
        // MAX
        // =====================================================

        if (isMax)
        {
            float value =
                float.NegativeInfinity;

            foreach (CardInstance card in playableCards)
            {
                SimGameState next = state.Clone();

                SimEntity cur =
                    next.selfTurn ? next.self : next.opponent;

                SimEntity opp =
                    next.selfTurn ? next.opponent : next.self;

                CardInstance simCard =
                    FindCardByID(cur, card.instanceID);

                if (simCard == null)
                    continue;

                SimBattleHelper.TryUseCard(
                    cur, opp, simCard);

                if (!HasPlayableCard(cur))
                {
                    EndTurn(next);
                }

                value = Mathf.Max(
                    value,
                    AlphaBeta(next, depth - 1, alpha, beta, false));

                alpha = Mathf.Max(alpha, value);

                // ===== PRUNING =====
                if (alpha >= beta)
                {
                    break;
                }
            }

            return value;
        }

        // =====================================================
        // MIN
        // =====================================================

        else
        {
            float value =
                float.PositiveInfinity;

            foreach (CardInstance card in playableCards)
            {
                SimGameState next = state.Clone();

                SimEntity cur =
                    next.selfTurn ? next.self : next.opponent;

                SimEntity opp =
                    next.selfTurn ? next.opponent : next.self;

                CardInstance simCard =
                    FindCardByID(cur, card.instanceID);

                if (simCard == null)
                    continue;

                SimBattleHelper.TryUseCard(
                    cur, opp, simCard);

                if (!HasPlayableCard(cur))
                {
                    EndTurn(next);
                }

                value = Mathf.Min(
                    value,
                    AlphaBeta(next, depth - 1, alpha, beta, true));

                beta = Mathf.Min(beta, value);

                // ===== PRUNING =====
                if (alpha >= beta)
                {
                    break;
                }
            }

            return value;
        }
    }

    // =====================================================
    // 평가 함수(미니맥스와 동일)
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

    private List<CardInstance> GetPlayableCards(Entity entity)
    {
        List<CardInstance> result = new();

        foreach (CardInstance card in entity.hand)
        {
            if (card.currentCost <= entity.CurrentEnergy)
                result.Add(card);
        }

        return result;
    }

    private List<CardInstance> GetPlayableCards(SimEntity entity)
    {
        List<CardInstance> result = new();

        foreach (CardInstance card in entity.hand)
        {
            if (card.currentCost <= entity.CurrentEnergy)
                result.Add(card);
        }

        return result;
    }

    private bool HasPlayableCard(SimEntity entity)
    {
        foreach (CardInstance card in entity.hand)
        {
            if (card.currentCost <= entity.CurrentEnergy)
                return true;
        }

        return false;
    }

    private CardInstance FindCardByID(SimEntity entity, int id)
    {
        foreach (CardInstance card in entity.hand)
        {
            if (card.instanceID == id)
                return card;
        }

        return null;
    }

    private void EndTurn(SimGameState sim)
    {
        SimEntity current =
            sim.selfTurn ? sim.self : sim.opponent;

        current.CurrentEnergy = 0;
        current.DiscardHand();
        current.TickStatuses();

        sim.selfTurn = !sim.selfTurn;
        sim.turnCount++;

        SimEntity next =
            sim.selfTurn ? sim.self : sim.opponent;

        next.Block = 0;
        next.CurrentEnergy = next.MaxEnergy;
        next.DrawCards(5);
    }

    private float Tanh(float x)
    {
        float ePos = Mathf.Exp(x);
        float eNeg = Mathf.Exp(-x);

        return (ePos - eNeg) / (ePos + eNeg);
    }
}