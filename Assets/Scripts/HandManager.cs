using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 손패 UI 관리 (카드 생성 / 정렬 / 데이터 반영)
/// </summary>
public class HandManager : MonoBehaviour
{
    public static HandManager instance;

    [Header("Reference")]
    public Transform handArea;
    public GameObject cardPrefab;

    [HideInInspector] public List<CardView> cards = new List<CardView>();

    public float spacing = 150f;
    public float fanAngle = 5f;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public CardView CreateCard(CardInstance cardInstance)
    {
        GameObject obj = Instantiate(cardPrefab, handArea);

        CardView view = obj.GetComponent<CardView>();
        CardUI ui = obj.GetComponent<CardUI>();

        view.SetCard(cardInstance);
        ui.Setup(cardInstance);

        return view;
    }

    // =====================================================
    // 손패 새로 생성
    // =====================================================

    public void RefreshHand()
    {
        foreach (Transform child in handArea)
        {
            Destroy(child.gameObject);
        }

        cards.Clear();

        foreach (CardInstance cardInstance in DBTest.instance.hand)
        {
            GameObject obj = Instantiate(cardPrefab, handArea);

            CardView view = obj.GetComponent<CardView>();
            view.SetCard(cardInstance);

            AddCard(view);
        }

        UpdateLayout();
    }

    // =====================================================
    // 카드 추가
    // =====================================================

    public void AddCard(CardView card)
    {
        cards.Add(card);
        card.transform.SetParent(handArea, false);
        UpdateLayout();
    }

    // =====================================================
    // 카드 제거
    // =====================================================

    public void RemoveCard(CardView card)
    {
        cards.Remove(card);
        Destroy(card.gameObject);
        UpdateLayout();
    }

    // =====================================================
    // 레이아웃 (목표 위치만 설정)
    // =====================================================

    public void UpdateLayout()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            float offset = i - (cards.Count - 1) / 2f;

            cards[i].targetPos = new Vector2(offset * spacing, 0);
            cards[i].targetRot = Quaternion.Euler(0, 0, -offset * fanAngle);

            cards[i].transform.SetSiblingIndex(i);
        }
    }
}