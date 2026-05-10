using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 덱, 뽑을 카드 더미, 버린 카드 더미, 소멸된 카드 더미 버튼을 눌렀을 때 보여지는 팝업을 관리하는 class
/// </summary>
public class CardListUIManager : MonoBehaviour
{
    public static CardListUIManager instance;

    [Header("Popup")]
    public GameObject popup;

    [Header("Card Spawn")]
    public Transform contentParent;
    public GameObject cardShowPrefab;

    private List<GameObject> spawnedCards = new List<GameObject>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        popup.SetActive(false);
    }

    // =====================================================
    // 카드 리스트 표시
    // =====================================================

    public void ShowCardList(List<CardInstance> cards)
    {
        popup.SetActive(true);

        ClearCards();

        foreach (CardInstance card in cards)
        {
            GameObject obj =
                Instantiate(cardShowPrefab, contentParent);

            CardUI cardUI =
                obj.GetComponent<CardUI>();

            cardUI.Setup(card);

            spawnedCards.Add(obj);
        }
    }

    // =====================================================
    // 팝업 닫기
    // =====================================================

    public void ClosePopup()
    {
        popup.SetActive(false);
    }

    // =====================================================
    // 카드 제거
    // =====================================================

    private void ClearCards()
    {
        foreach (GameObject obj in spawnedCards)
        {
            Destroy(obj);
        }

        spawnedCards.Clear();
    }

    // =====================================================
    // 버튼용 함수
    // =====================================================

    public void OpenPlayerDeck()
    {
        ShowCardList(GameData.instance.player.deck);
    }

    /// <summary>
    /// 드로우 파일은 랜덤한 순서로 보여지도록 함
    /// </summary>
    /// <param name="list"></param>
    private void ShuffleList(List<CardInstance> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex =
                Random.Range(i, list.Count);

            CardInstance temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    public void OpenDrawPile()
    {
        List<CardInstance> shuffledList = new List<CardInstance>(GameData.instance.player.drawPile);
        ShuffleList(shuffledList);
        ShowCardList(GameData.instance.player.drawPile);
    }

    public void OpenDiscardPile()
    {
        ShowCardList(GameData.instance.player.discardPile);
    }

    public void OpenExhaustPile()
    {
        ShowCardList(GameData.instance.player.exhaustPile);
    }
}