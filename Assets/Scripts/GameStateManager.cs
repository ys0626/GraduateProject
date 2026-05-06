using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // 셔플
    // ─────────────────────────────────────────────
    public void ShuffleDeck(List<CardInstance> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(i, deck.Count);
            CardInstance temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    // ─────────────────────────────────────────────
    // 드로우
    // ─────────────────────────────────────────────

    /// <summary>
    /// deck → hand로 count만큼 드로우
    /// deck이 비면 discardPile을 리셔플해서 사용
    /// </summary>
    public GameState DrawCards(GameState state, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // 손패 최대치 제한 (10장)
            if (state.hand.Count >= 10)
            {
                Debug.Log("손패가 가득 찼습니다.");
                break;
            }

            // 드로우 파일 비었으면 버린 카드 더미 리셔플
            if (state.deck.Count == 0)
            {
                if (state.discardPile.Count == 0)
                {
                    Debug.Log("드로우할 카드가 없습니다.");
                    break;
                }

                state.deck.AddRange(state.discardPile);
                state.discardPile.Clear();
                ShuffleDeck(state.deck);

                Debug.Log("버린 카드 더미를 리셔플했습니다.");
            }

            CardInstance card = state.deck[0];
            state.deck.RemoveAt(0);
            state.hand.Add(card);
        }

        return state;
    }

    // ─────────────────────────────────────────────
    // 카드 사용
    // ─────────────────────────────────────────────

    /// <summary>
    /// 카드 사용: 에너지 소모 → 효과 적용 → 손패 제거
    /// isPlayerTurn으로 공격자/피공격자 구분
    /// </summary>
    public GameState ApplyAction(GameState state, CardInstance card)
    {
        // 에너지 체크
        if (state.currentEnergy < card.currentCost)
        {
            Debug.Log("에너지가 부족합니다");
            return state;
        }

        // 에너지 소모
        state.currentEnergy -= card.currentCost;

        // Factory로 Effect 생성 후 실행
        ICardEffect effect = CardEffectFactory.Create(card);
        effect?.Execute(state);

        // Exhaust 카드 처리
        if (card.exhaust)
        {
            state.hand.Remove(card);
            state.exhaustPile.Add(card);
            Debug.Log($"[{card.data.cardName}] 소멸됨");
        }
        else
        {
            state.hand.Remove(card);
            state.discardPile.Add(card);
        }

        return state;
    }
}