using UnityEngine;

/// <summary>
/// 적의 손패, 뽑을 카드 더미, 버린 카드 더미, 소멸된 카드 더미의 변화를 관리하는 class
/// </summary>
public class EnemyDeckManager : MonoBehaviour
{
    public static EnemyDeckManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 전투 시작 전 적의 덱 세팅(덱의 카드들을 드로우 파일에 추가하고 섞기)
    /// </summary>
    public void InitEnemyDeck()
    {
        InitializeEnemyDeck();
        ShuffleDrawPile();
    }

    /// <summary>
    /// 맨 처음, 적의 뽑을 카드 더미에 적의 덱의 모든 카드들을 추가
    /// </summary>
    private void InitializeEnemyDeck()
    {
        //혹시 모를 이전에 저장된 값 삭제
        GameData.instance.enemy.hand.Clear();
        GameData.instance.enemy.drawPile.Clear();
        GameData.instance.enemy.discardPile.Clear();
        GameData.instance.enemy.exhaustPile.Clear();

        foreach (CardInstance cardInstance in GameData.instance.enemy.deck)
        {
            GameData.instance.enemy.drawPile.Add(new CardInstance(cardInstance));
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
        // 1. 손패 최대 제한 (10장)
        if (GameData.instance.enemy.hand.Count >= 10)
            return;

        // 2. 드로우 더미가 비어있으면 재구성 시도
        if (GameData.instance.enemy.drawPile.Count == 0)
        {
            // discard도 없으면 드로우 불가
            if (GameData.instance.enemy.discardPile.Count == 0)
                return;

            ReshuffleDiscardPile();
        }

        // 3. 재구성 후에도 없으면 종료 (안전장치)
        if (GameData.instance.enemy.drawPile.Count == 0)
            return;

        // 4. 손패 재확인 (리셋 상황 대비)
        if (GameData.instance.enemy.hand.Count >= 10)
            return;

        CardInstance card = GameData.instance.enemy.drawPile[0];

        GameData.instance.enemy.drawPile.Remove(card);
        GameData.instance.enemy.hand.Add(card);
    }

    // =====================================================
    // 카드 버리기
    // =====================================================
    public void DiscardCard(CardInstance card)
    {
        //손패에서 제거
        GameData.instance.enemy.hand.Remove(card);

        // 데이터 이동
        GameData.instance.enemy.discardPile.Add(card);
    }

    // =====================================================
    // 카드 소멸
    // =====================================================

    public void ExhaustCard(CardInstance card)
    {
        //손패에서 제거
        GameData.instance.enemy.hand.Remove(card);

        // 데이터 이동
        GameData.instance.enemy.exhaustPile.Add(card);
    }

    // =====================================================
    // 뽑을 카드 더미 섞기
    // =====================================================

    public void ShuffleDrawPile()
    {
        for (int i = 0; i < GameData.instance.enemy.drawPile.Count; i++)
        {
            int randomIndex =
                Random.Range(i, GameData.instance.enemy.drawPile.Count);

            CardInstance temp = GameData.instance.enemy.drawPile[i];
            GameData.instance.enemy.drawPile[i] = GameData.instance.enemy.drawPile[randomIndex];
            GameData.instance.enemy.drawPile[randomIndex] = temp;
        }
    }


    // =====================================================
    // 뽑을 카드 더미에서 카드가 부족한 경우,
    //  버린 카드 더미의 카드들을 뽑을 카드 더미로 보내고 뽑을 카드 더미를 섞기
    // =====================================================
    private void ReshuffleDiscardPile()
    {
        foreach (CardInstance card in GameData.instance.enemy.discardPile)
        {
            GameData.instance.enemy.drawPile.Add(card);
        }

        GameData.instance.enemy.discardPile.Clear();

        ShuffleDrawPile();
    }
}