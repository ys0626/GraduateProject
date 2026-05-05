using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 손패, 드로우 파일, 버린 카드 더미, 소멸된 카드 더미의 변화를 관리하는 class
/// </summary>
public class DeckManager : MonoBehaviour
{
    public static DeckManager instance;
    
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        InitializeDeck();

        ShuffleDrawPile();

        BattleManagerTest.instance.StartBattle();
    }

    // =====================================================
    // Initialize
    // =====================================================

    private void InitializeDeck()
    {
        foreach (CardInstance cardInstance in DBTest.instance.playerDeck)
        {
            AddToDrawPile(new CardInstance(cardInstance));
        }
    }

    // =====================================================
    // Draw
    // =====================================================

    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            DrawCard();
        }
    }

    public void DrawCard()
    {
        if (DBTest.instance.drawPile.Count == 0)
        {
            ReshuffleDiscardPile();
        }

        if (DBTest.instance.drawPile.Count == 0)
        {
            return;
        }

        CardInstance card = DBTest.instance.drawPile[0];

        RemoveFromDrawPile(card);

        // 1. 데이터 추가
        DBTest.instance.hand.Add(card);

        // 2. UI 생성
        CardView view = HandManager.instance.CreateCard(card);
        HandManager.instance.AddCard(view);

        UpdateCounts();
    }

    // =====================================================
    // Discard
    // =====================================================

    public void DiscardCard(CardInstance card)
    {
        DBTest.instance.hand.Remove(card);

        // UI 제거
        CardView view = HandManager.instance.cards.Find(c => c.card == card);

        if (view != null)
        {
            HandManager.instance.RemoveCard(view);

            Destroy(view.gameObject);
        }

        // 데이터 이동
        AddToDiscardPile(card);

        UpdateCounts();
    }

    // =====================================================
    // Exhaust
    // =====================================================

    public void ExhaustCard(CardInstance card)
    {
        DBTest.instance.hand.Remove(card);

        CardView view = HandManager.instance.cards
            .Find(c => c.card == card);

        if (view != null)
        {
            HandManager.instance.RemoveCard(view);
        }

        AddToExhaustPile(card);

        UpdateCounts();
    }

    // =====================================================
    // Shuffle
    // =====================================================

    public void ShuffleDrawPile()
    {
        for (int i = 0; i < DBTest.instance.drawPile.Count; i++)
        {
            int randomIndex =
                Random.Range(i, DBTest.instance.drawPile.Count);

            CardInstance temp = DBTest.instance.drawPile[i];

            DBTest.instance.drawPile[i] = DBTest.instance.drawPile[randomIndex];

            DBTest.instance.drawPile[randomIndex] = temp;
        }
    }

    private void ReshuffleDiscardPile()
    {
        foreach (CardInstance card in DBTest.instance.discardPile)
        {
            AddToDrawPile(card);
        }

        DBTest.instance.discardPile.Clear();

        ShuffleDrawPile();
        UpdateCounts();
    }

    // =====================================================
    // UI Count Update
    // =====================================================

    private void UpdateCounts()
    {
        DBTest.instance.DrawPileCount = DBTest.instance.drawPile.Count;

        DBTest.instance.DiscardPileCount = DBTest.instance.discardPile.Count;

        DBTest.instance.ExhaustPileCount = DBTest.instance.exhaustPile.Count;
    }

    // =====================================================
    // 덱에 카드 추가
    // =====================================================

    public void AddCardToPlayerDeck(CardData cardData)
    {
        DBTest.instance.playerDeck.Add(new CardInstance(cardData));
        DBTest.instance.DeckCount = DBTest.instance.playerDeck.Count;
    }

    // =====================================================
    // 드로우 파일에서 카드 추가, 제거
    // =====================================================
    public void AddToDrawPile(CardInstance card)
    {
        DBTest.instance.drawPile.Add(card);
        DBTest.instance.DrawPileCount = DBTest.instance.drawPile.Count;
    }

    public void RemoveFromDrawPile(CardInstance card)
    {
        DBTest.instance.drawPile.Remove(card);
        DBTest.instance.DrawPileCount = DBTest.instance.drawPile.Count;
    }

    // =====================================================
    // 버린 카드 더미에서 카드 추가, 제거
    // =====================================================
    public void AddToDiscardPile(CardInstance card)
    {
        DBTest.instance.discardPile.Add(card);
        DBTest.instance.DiscardPileCount = DBTest.instance.discardPile.Count;
    }

    public void RemoveFromDiscardPile(CardInstance card)
    {
        DBTest.instance.discardPile.Remove(card);
        DBTest.instance.DiscardPileCount = DBTest.instance.discardPile.Count;
    }

    // =====================================================
    // 소멸된 카드 더미에서 카드 추가, 제거
    // =====================================================
    public void AddToExhaustPile(CardInstance card)
    {
        DBTest.instance.exhaustPile.Add(card);
        DBTest.instance.ExhaustPileCount = DBTest.instance.exhaustPile.Count;
    }

    public void RemoveFromExhaustPile(CardInstance card)
    {
        DBTest.instance.exhaustPile.Remove(card);
        DBTest.instance.ExhaustPileCount = DBTest.instance.exhaustPile.Count;
    }
}