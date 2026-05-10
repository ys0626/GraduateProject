using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 손패에 있는 카드들 중에서 랜덤으로 사용하는 랜덤 알고리즘
/// </summary>
public class RandomController : IEntityController
{
    public CardInstance SelectCard(Entity entity)
    {
        // =====================================================
        // 현재 에너지로 사용 가능한 카드들만 수집
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
        // 사용 가능한 카드가 없다면 턴 종료
        // =====================================================

        if (playableCards.Count == 0)
        {
            return null;
        }

        // =====================================================
        // 사용 가능한 카드 중 랜덤 선택
        // =====================================================

        int randomIndex =
            Random.Range(0, playableCards.Count);

        return playableCards[randomIndex];
    }
}