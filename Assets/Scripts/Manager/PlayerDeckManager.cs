using UnityEngine;
using static UIManager;

/// <summary>
/// 플레이어의 손패, 뽑을 카드 더미, 버린 카드 더미, 소멸된 카드 더미의 변화를 관리하는 class
/// </summary>
public class PlayerDeckManager : MonoBehaviour
{
    public static PlayerDeckManager instance;
    
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 전투 시작 전 플레이어의 덱 세팅(덱의 카드들을 드로우 파일에 추가하고 섞기)
    /// </summary>
    public void InitPlayerDeck()
    {
        InitializePlayerDeck();
        ShuffleDrawPile();
    }

    /// <summary>
    /// 맨 처음, 플레이어의 뽑을 카드 더미에 플레이어의 덱의 모든 카드들의 복사본을 추가
    /// </summary>
    private void InitializePlayerDeck()
    {
        //혹시 모를 이전에 저장된 값 삭제
        GameData.instance.player.hand.Clear();
        GameData.instance.player.drawPile.Clear();
        GameData.instance.player.discardPile.Clear();
        GameData.instance.player.exhaustPile.Clear();

        foreach (CardInstance cardInstance in GameData.instance.player.deck)
        {
            AddCardToPlayerDrawPile(new CardInstance(cardInstance));
        }
    }

    // =====================================================
    // 카드 드로우
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
        // 1. 손패 최대 제한
        if (GameData.instance.player.hand.Count >= 10)
        {
            UIManager.instance.ShowWarning(WarningType.HandFull);
            return;
        }

        // 2. 드로우가 비어있으면 재구성 시도
        if (GameData.instance.player.drawPile.Count == 0)
        {
            // discard도 없으면 완전히 드로우 불가
            if (GameData.instance.player.discardPile.Count == 0)
            {
                UIManager.instance.ShowWarning(WarningType.NoCardsToDraw);
                return;
            }

            ReshuffleDiscardPile();
        }

        // 3. 재구성 후에도 드로우가 없으면 종료 (안전장치)
        if (GameData.instance.player.drawPile.Count == 0)
        {
            UIManager.instance.ShowWarning(WarningType.NoCardsToDraw);
            return;
        }

        // 4. 다시 손패 체크 (리셋 이후 상황 대비)
        if (GameData.instance.player.hand.Count >= 10)
        {
            UIManager.instance.ShowWarning(WarningType.HandFull);
            return;
        }

        // 5. 카드 뽑기
        CardInstance card = GameData.instance.player.drawPile[0];

        RemoveCardFromPlayerDrawPile(card);

        GameData.instance.player.hand.Add(card);

        CardView view = HandManager.instance.CreateCard(card);
        HandManager.instance.AddCard(view);

        UpdateCounts();
    }

    // =====================================================
    // 카드 버리기
    // =====================================================
    public void DiscardCard(CardInstance card)
    {
        //손패에서 제거
        GameData.instance.player.hand.Remove(card);

        // UI 제거
        CardView view = HandManager.instance.cards.Find(c => c.card == card);

        if (view != null)
        {
            HandManager.instance.RemoveCard(view);
        }

        // 데이터 이동
        AddCardToPlayerDiscardPile(card);

        UpdateCounts();
    }

    // =====================================================
    // 카드 소멸
    // =====================================================

    public void ExhaustCard(CardInstance card)
    {
        //손패에서 제거
        GameData.instance.player.hand.Remove(card);

        // UI 제거
        CardView view = HandManager.instance.cards.Find(c => c.card == card);

        if (view != null)
        {
            HandManager.instance.RemoveCard(view);
        }

        // 데이터 이동
        AddCardToPlayerExhaustPile(card);

        UpdateCounts();
    }

    // =====================================================
    // 뽑을 카드 더미 섞기
    // =====================================================

    public void ShuffleDrawPile()
    {
        for (int i = 0; i < GameData.instance.player.drawPile.Count; i++)
        {
            int randomIndex =
                Random.Range(i, GameData.instance.player.drawPile.Count);

            CardInstance temp = GameData.instance.player.drawPile[i];
            GameData.instance.player.drawPile[i] = GameData.instance.player.drawPile[randomIndex];
            GameData.instance.player.drawPile[randomIndex] = temp;
        }
    }


    // =====================================================
    // 뽑을 카드 더미에서 카드가 부족한 경우,
    //  버린 카드 더미의 카드들을 뽑을 카드 더미로 보내고 뽑을 카드 더미를 섞기
    // =====================================================
    private void ReshuffleDiscardPile()
    {
        foreach (CardInstance card in GameData.instance.player.discardPile)
        {
            AddCardToPlayerDrawPile(card);
        }

        GameData.instance.player.discardPile.Clear();

        ShuffleDrawPile();
        UpdateCounts();
    }

    

    /// <summary>
    /// 뽑을 카드 더미 수, 버린 카드 더미 수, 소멸된 카드 더미 수를 업데이트
    /// </summary>
    private void UpdateCounts()
    {
        GameData.instance.player.DrawPileCount = GameData.instance.player.drawPile.Count;
        GameData.instance.player.DiscardPileCount = GameData.instance.player.discardPile.Count;
        GameData.instance.player.ExhaustPileCount = GameData.instance.player.exhaustPile.Count;
    }




    // =====================================================
    // 플레이어의 덱에 카드 추가, 제거
    // =====================================================
    public void AddCardToPlayerDeck(CardData cardData)
    {
        GameData.instance.player.deck.Add(new CardInstance(cardData));
        GameData.instance.player.DeckCount = GameData.instance.player.deck.Count;
    }

    public void RemoveCardFromPlayerDeck(CardInstance card)
    {
        GameData.instance.player.deck.Remove(card);
        GameData.instance.player.DeckCount = GameData.instance.player.deck.Count;
    }

    // =====================================================
    // 플레이어의 뽑을 카드 더미에 카드 추가, 제거
    // =====================================================
    public void AddCardToPlayerDrawPile(CardInstance card)
    {
        GameData.instance.player.drawPile.Add(card);
        GameData.instance.player.DrawPileCount = GameData.instance.player.drawPile.Count;
    }

    public void RemoveCardFromPlayerDrawPile(CardInstance card)
    {
        GameData.instance.player.drawPile.Remove(card);
        GameData.instance.player.DrawPileCount = GameData.instance.player.drawPile.Count;
    }

    // =====================================================
    // 플레이어의 버린 카드 더미에 카드 추가, 제거
    // =====================================================
    public void AddCardToPlayerDiscardPile(CardInstance card)
    {
        GameData.instance.player.discardPile.Add(card);
        GameData.instance.player.DiscardPileCount = GameData.instance.player.discardPile.Count;
    }

    public void RemoveCardFromPlayerDiscardPile(CardInstance card)
    {
        GameData.instance.player.discardPile.Remove(card);
        GameData.instance.player.DiscardPileCount = GameData.instance.player.discardPile.Count;
    }

    // =====================================================
    // 플레이어의 소멸된 카드 더미에 카드 추가, 제거
    // =====================================================
    public void AddCardToPlayerExhaustPile(CardInstance card)
    {
        GameData.instance.player.exhaustPile.Add(card);
        GameData.instance.player.ExhaustPileCount = GameData.instance.player.exhaustPile.Count;
    }

    public void RemoveCardFromPlayerExhaustPile(CardInstance card)
    {
        GameData.instance.player.exhaustPile.Remove(card);
        GameData.instance.player.ExhaustPileCount = GameData.instance.player.exhaustPile.Count;
    }
}