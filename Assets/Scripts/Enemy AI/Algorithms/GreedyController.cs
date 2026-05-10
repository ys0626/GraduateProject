using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 사용 가능한 카드들 중 가장 코스트가 높은 카드를 우선적으로 사용하는 탐욕 알고리즘
/// 같은 코스트의 카드가 여러 개라면 그 중 랜덤하게 선택한다
/// </summary>
public class GreedyController : IEntityController
{
    public CardInstance SelectCard(Entity entity)
    {
        // =====================================================
        // 현재 에너지로 사용 가능한 카드들 수집
        // =====================================================

        List<CardInstance> playableCards =
            new List<CardInstance>();

        foreach (CardInstance card in entity.hand)
        {
            if (card.currentCost <= entity.CurrentEnergy)
            {
                playableCards.Add(card);
            }
        }

        // =====================================================
        // 사용 가능한 카드가 없으면 턴 종료
        // =====================================================

        if (playableCards.Count == 0)
        {
            return null;
        }

        // =====================================================
        // 사용 가능한 카드 중 최대 코스트 찾기
        // =====================================================

        int maxCost = -1;

        foreach (CardInstance card in playableCards)
        {
            if (card.currentCost > maxCost)
            {
                maxCost = card.currentCost;
            }
        }

        // =====================================================
        // 최대 코스트 카드들만 수집
        // =====================================================

        List<CardInstance> highestCostCards =
            new List<CardInstance>();

        foreach (CardInstance card in playableCards)
        {
            if (card.currentCost == maxCost)
            {
                highestCostCards.Add(card);
            }
        }

        // =====================================================
        // 최대 코스트 카드들 중 랜덤 선택
        // =====================================================

        int randomIndex =
            Random.Range(0, highestCostCards.Count);

        return highestCostCards[randomIndex];
    }
}