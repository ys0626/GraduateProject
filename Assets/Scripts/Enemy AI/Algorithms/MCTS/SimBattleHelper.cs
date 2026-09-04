using System.Collections.Generic;

/// <summary>
/// MCTS / 시뮬레이션 전용 전투 처리
/// </summary>
public static class SimBattleHelper
{
    /// <summary>
    /// 카드 사용 시도
    /// </summary>
    public static bool TryUseCard(
        SimEntity user,
        SimEntity target,
        CardInstance card)
    {
        // =====================================================
        // 카드 사용 가능 여부 확인
        // =====================================================

        // 실제 손패에 존재해야 함
        if (!user.hand.Contains(card))
        {
            return false;
        }

        // 에너지 부족
        if (!user.TryUseEnergy(card.currentCost))
        {
            return false;
        }

        // =====================================================
        // 카드 효과 실행
        // =====================================================

        ICardEffect effect =
            CardEffectFactory.Create(card);

        effect?.Execute(user, target);

        if (card.data.cardType == CardType.Attack && user.DoubleTapCharges > 0)
        {
            effect?.Execute(user, target);
            user.DoubleTapCharges -= 1;
        }

        // =====================================================
        // 카드 처리
        // =====================================================

        // 파워 카드
        if (card.data.cardType == CardType.Power)
        {
            user.hand.Remove(card);
        }

        // 소멸 카드
        else if (card.exhaust)
        {
            user.ExhaustCard(card);
        }

        // 일반적인 카드
        else
        {
            user.DiscardCard(card);
        }

        return true;
    }
}