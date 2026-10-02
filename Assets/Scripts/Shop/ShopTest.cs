using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ShopScene 단독 테스트용 스크립트
/// GameManager 없이 ShopScene만 Play할 때 테스트용 GameData와 플레이어를 생성
/// GameManager가 작동하는 실제 플레이에서는 이 오브젝트를 비활성화해 둘 것
/// </summary>
[DefaultExecutionOrder(-100)]
public class ShopTest : MonoBehaviour
{
    // =====================================================
    // 테스트 설정
    // =====================================================

    [Header("테스트 플레이어 설정")]
    [SerializeField] private int startGold = 500;

    [Header("테스트 시작 덱 (플레이어 카드 풀)")]
    [SerializeField] private List<CardData> startingDeck;

    private Entity testPlayer;

    // =====================================================
    // 초기화
    // =====================================================

    private void Awake()
    {
        // 이미 GameData가 있으면(GameManager 작동 중) 실제 데이터를 덮어쓰지 않음
        if (GameData.instance != null)
        {
            Debug.LogWarning("[ShopTest] GameData가 이미 존재함 (GameManager 작동 중). ShopTest를 비활성화해 주세요. 테스트 데이터를 만들지 않음");
            return;
        }

        GameObject gameDataObject =
            new GameObject("GameData (ShopTest)");

        // AddComponent 시점에 GameData.Awake가 실행되어 instance가 설정됨
        GameData gameData =
            gameDataObject.AddComponent<GameData>();

        testPlayer =
            gameData.player;

        testPlayer.Gold = startGold;

        SetupStartingDeck();

        // 구매 결과 확인용 로그
        testPlayer.OnGoldChanged += LogGold;
        testPlayer.OnDeckCountChanged += LogDeckCount;

        Debug.Log($"[ShopTest] 테스트 플레이어 생성 | Gold: {testPlayer.Gold} | 시작 덱: {testPlayer.deck.Count}장");
    }

    private void OnDestroy()
    {
        if (testPlayer != null)
        {
            testPlayer.OnGoldChanged -= LogGold;
            testPlayer.OnDeckCountChanged -= LogDeckCount;
        }
    }

    // =====================================================
    // 로그
    // =====================================================

    private void LogGold(int gold)
    {
        Debug.Log($"[ShopTest] Gold: {gold}");
    }

    private void LogDeckCount(int count)
    {
        Debug.Log($"[ShopTest] 덱 카드 수: {count}");
    }

    // =====================================================
    // 시작 덱 세팅
    // =====================================================

    /// <summary>
    /// startingDeck에 등록된 카드로 테스트 플레이어의 덱을 채움
    /// </summary>
    private void SetupStartingDeck()
    {
        if (startingDeck == null || startingDeck.Count == 0)
        {
            Debug.LogWarning("[ShopTest] startingDeck이 비어 있음 (덱 없이 시작)");
            return;
        }

        foreach (CardData card in startingDeck)
        {
            if (card == null)
                continue;

            testPlayer.deck.Add(new CardInstance(card));
        }

        testPlayer.DeckCount = testPlayer.deck.Count;

        Debug.Log($"[ShopTest] 시작 덱 구성 완료: {testPlayer.deck.Count}장");
    }

    // =====================================================
    // 편의 기능 (Inspector 우클릭 메뉴, Play 중에만 사용)
    // =====================================================

    [ContextMenu("골드 +100")]
    private void AddGold()
    {
        if (testPlayer == null)
            return;

        testPlayer.Gold += 100;
    }

    [ContextMenu("골드 0으로")]
    private void ResetGold()
    {
        if (testPlayer == null)
            return;

        testPlayer.Gold = 0;
    }
}
