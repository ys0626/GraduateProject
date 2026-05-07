using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 손패, 드로우 파일, 버린 카드 더미, 소멸된 카드 더미의 UI 및 데이터 이동 관리
/// 전투 로직(셔플, 드로우 계산)은 GameStateManager에서 처리
/// </summary>
public class DeckManager : MonoBehaviour
{
    public static DeckManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 1. DBTest.playerDeck → drawPile로 복사
        InitializeDeck();

        // 2. BattleManager에 전달 (셔플은 BattleManager → GameStateManager에서 처리)
        Entity player = DBTest.instance.player;
        Entity enemy = DBTest.instance.enemy;

        // static이 아닌 Instance로 통일
        BattleManager.Instance.StartBattle(player, enemy, DBTest.instance.drawPile);
    }

    // =====================================================
    // Initialize
    // =====================================================

    private void InitializeDeck()
    {
        // drawPile 초기화 후 복사
        DBTest.instance.drawPile.Clear();

        foreach (CardInstance card in DBTest.instance.playerDeck)
        {
            // 복사본 추가 (원본 playerDeck 보존)
            AddToDrawPile(new CardInstance(card));
        }
    }

    // =====================================================
    // GameState 변경 시 UI 동기화 (BattleManager 이벤트 구독)
    // =====================================================

    private void OnEnable()
    {
        // Start 이후에 구독되므로 null 체크 + 안전하게 처리
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnStateUpdated += SyncWithGameState;
    }

    private void OnDisable()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnStateUpdated -= SyncWithGameState;
    }

    /// <summary>
    /// GameState가 바뀔 때마다 DBTest와 UI를 동기화
    /// </summary>
    private void SyncWithGameState(GameState state)
    {
        // DBTest.SyncFromGameState()로 일원화
        DBTest.instance.SyncFromGameState(state);

        // UI 동기화
        SyncHandUI(state.hand);
        UpdateCounts();
    }

    /// <summary>
    /// 손패 UI를 GameState.hand 기준으로 동기화
    /// </summary>
    private void SyncHandUI(List<CardInstance> hand)
    {
        if (HandManager.instance == null) return;

        // GameState에 있는데 UI에 없는 카드 → 추가
        foreach (CardInstance card in hand)
        {
            bool alreadyShown = HandManager.instance.cards
                .Exists(v => v.card == card);

            if (!alreadyShown)
            {
                CardView view = HandManager.instance.CreateCard(card);
                HandManager.instance.AddCard(view);
            }
        }

        // UI에 있는데 GameState에 없는 카드 → 제거
        List<CardView> toRemove = HandManager.instance.cards
            .FindAll(v => !hand.Contains(v.card));

        foreach (CardView view in toRemove)
        {
            HandManager.instance.RemoveCard(view);
            Destroy(view.gameObject);
        }
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
    // 외부에서 직접 카드 조작이 필요할 때만 사용
    // (보상 화면에서 덱에 카드 추가 등)
    // =====================================================

    /// <summary>
    /// 보상 등으로 플레이어 덱에 카드 추가할 때 사용
    /// </summary>
    public void AddCardToPlayerDeck(CardData cardData)
    {
        DBTest.instance.playerDeck.Add(new CardInstance(cardData));
        DBTest.instance.DeckCount = DBTest.instance.playerDeck.Count;
    }

    // =====================================================
    // 드로우 파일
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
    // 버린 카드 더미
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
    // 소멸된 카드 더미
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