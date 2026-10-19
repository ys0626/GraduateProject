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

    [Header("Setting")]
    [SerializeField] float radius = 400f;
    [SerializeField] float spreadPerCard = 18f;
    [SerializeField] float maxAngleSpread = 160f;

    [HideInInspector] public List<CardView> cards = new List<CardView>();


    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        EnsureVisibleHandLayout();
    }

    private void EnsureVisibleHandLayout()
    {
        if (handArea is RectTransform handRect &&
            handRect.anchoredPosition.y < -1000f)
        {
            // 이전 씬 값(-3900)은 반지름 4000 전용 오프셋이었다.
            // 현재 카드 반지름(400)에서는 화면 밖으로 나가므로 하단에 배치한다.
            handRect.anchoredPosition = new Vector2(
                handRect.anchoredPosition.x,
                -300f
            );
        }

        // 기존 씬에 직렬화된 작은 값도 카드가 겹치지 않게 보정한다.
        spreadPerCard = Mathf.Max(spreadPerCard, 18f);
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

        foreach (CardInstance cardInstance in GameData.instance.player.hand)
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
    // 레이아웃
    // =====================================================
    public void UpdateLayout()
    {
        int count = cards.Count;
        if (count == 0) return;

        // 1. 전체 펼침 각도 계산
        float angleSpread = count * spreadPerCard;
        angleSpread = Mathf.Min(maxAngleSpread, angleSpread);

        // 2. 시작 각도 (중앙 기준 대칭)
        float startAngle = -angleSpread * 0.5f;

        // 3. 카드 간 간격
        float step = (count > 1) ? angleSpread / (count - 1) : 0f;

        // 4. 카드 배치
        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + step * i;
            float rad = angle * Mathf.Deg2Rad;

            float x = Mathf.Sin(rad) * radius;
            float y = Mathf.Cos(rad) * radius;

            Vector2 pos = new Vector2(x, y);

            cards[i].targetPos = pos;
            cards[i].targetRot = Quaternion.Euler(0, 0, -angle);

            cards[i].transform.SetSiblingIndex(i);
        }
    }


}
