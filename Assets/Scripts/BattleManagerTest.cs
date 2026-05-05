using System.Collections;
using UnityEngine;

/// <summary>
/// 테스트용 배틀매니저(대체필요)
/// </summary>
public class BattleManagerTest : MonoBehaviour
{
    public static BattleManagerTest instance;

    private bool isDrawing;

    [Header("Turn Setting")]
    public int drawPerTurn = 5;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // =====================================================
    // 전투 시작
    // =====================================================

    public void StartBattle()
    {
        StartPlayerTurn();
    }

    // =====================================================
    // 플레이어 턴 시작
    // =====================================================

    private void StartPlayerTurn()
    {
        RecoverEnergy();

        StartCoroutine(DrawHandRoutine());
    }

    private IEnumerator DrawHandRoutine()
    {
        for (int i = 0; i < drawPerTurn; i++)
        {
            DeckManager.instance.DrawCard();

            yield return new WaitForSeconds(0.15f);
        }

        isDrawing = false;
    }

    // =====================================================
    // 턴 종료
    // =====================================================

    public void EndTurn()
    {
        if (isDrawing) return;

        StartCoroutine(EndTurnRoutine());
    }

    // =====================================================
    // 손패 버리기
    // =====================================================

    private IEnumerator EndTurnRoutine()
    {
        isDrawing = true;

        yield return StartCoroutine(DiscardHandRoutine());

        yield return new WaitForSeconds(0.2f);

        StartPlayerTurn();
    }

    private IEnumerator DiscardHandRoutine()
    {
        HandManager hand = HandManager.instance;

        while (hand.cards.Count > 0)
        {
            CardView cardView =
                hand.cards[hand.cards.Count - 1];

            DeckManager.instance.DiscardCard(cardView.card);

            yield return new WaitForSeconds(0.08f);
        }
    }

    // =====================================================
    // 에너지 회복
    // =====================================================

    private void RecoverEnergy()
    {
        DBTest.instance.CurrentEnergy = DBTest.instance.MaxEnergy;
    }
}