using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardPopupManager : MonoBehaviour
{
    public static CardPopupManager instance;

    public event Action<CardData> OnUpgradeCardSelected;
    public event Action<CardInstance> OnUpgradeCompleted;

    [Header("Popup")]
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private Button popupBackgroundButton;
    [SerializeField] private TMP_Text popupTitleText;

    [Header("Card List")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private GameObject cardSelectionPrefab;

    [Header("Upgrade")]
    [SerializeField] private Button upgradeSelectedButton;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.2f;

    private readonly List<GameObject> spawnedCards =
        new List<GameObject>();

    private readonly List<CardInstance> cachedDrawOrder =
        new List<CardInstance>();

    private readonly List<CardInstance> requestedCards =
        new List<CardInstance>();

    private readonly List<CardInstance> displayedCards =
        new List<CardInstance>();

    private Coroutine transitionCoroutine;
    private Tween fadeTween;

    private string requestedPopupId;
    private string displayedPopupId;

    private bool requestedAllowCardClick;
    private bool displayedAllowCardClick;

    private bool isOpen;
    private bool isClosing;

    private CardSelectionUI selectedUpgradeUI;
    private CardInstance selectedUpgradeCard;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (popupCanvasGroup == null && popupRoot != null)
        {
            popupCanvasGroup =
                popupRoot.GetComponent<CanvasGroup>();
        }

        if (popupBackgroundButton != null)
        {
            popupBackgroundButton.onClick.AddListener(ClosePopup);
        }

        if (upgradeSelectedButton != null)
        {
            upgradeSelectedButton.onClick.AddListener(
                UpgradeSelectedCard
            );

            upgradeSelectedButton.interactable = false;
            upgradeSelectedButton.gameObject.SetActive(false);
        }

        if (popupCanvasGroup != null)
        {
            popupCanvasGroup.alpha = 0f;
            popupCanvasGroup.interactable = false;
            popupCanvasGroup.blocksRaycasts = false;
        }

        if (popupRoot != null)
        {
            popupRoot.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        if (popupBackgroundButton != null)
        {
            popupBackgroundButton.onClick.RemoveListener(ClosePopup);
        }

        if (upgradeSelectedButton != null)
        {
            upgradeSelectedButton.onClick.RemoveListener(
                UpgradeSelectedCard
            );
        }

        fadeTween?.Kill();

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        if (instance == this)
        {
            instance = null;
        }
    }

    // =====================================================
    // 공개 팝업 함수
    // =====================================================

    public void OpenDrawPile()
    {
        Entity player = GetPlayer();

        if (player == null)
            return;

        RefreshCachedDrawOrder(player.drawPile);

        ShowCards(
            "DrawPile",
            "뽑을 카드",
            cachedDrawOrder,
            false
        );
    }

    public void OpenDiscardPile()
    {
        Entity player = GetPlayer();

        if (player == null)
            return;

        ShowCards(
            "DiscardPile",
            "버린 카드",
            player.discardPile,
            false
        );
    }

    public void OpenExhaustPile()
    {
        Entity player = GetPlayer();

        if (player == null)
            return;

        ShowCards(
            "ExhaustPile",
            "소멸된 카드",
            player.exhaustPile,
            false
        );
    }

    public void OpenDeck()
    {
        Entity player = GetPlayer();

        if (player == null)
            return;

        ShowCards(
            "PlayerDeck",
            "플레이어 덱",
            player.deck,
            false
        );
    }

    public void OpenUpgradableDeck()
    {
        Entity player = GetPlayer();

        if (player == null)
            return;

        List<CardInstance> upgradableCards =
            new List<CardInstance>();

        foreach (CardInstance card in player.deck)
        {
            if (card == null || card.data == null)
                continue;

            if (!card.upgraded)
            {
                upgradableCards.Add(card);
            }
        }

        if (upgradableCards.Count == 0)
        {
            Debug.LogWarning(
                "[CardPopupManager] 강화할 수 있는 카드가 없습니다.",
                this
            );

            return;
        }

        ShowCards(
            "UpgradableDeck",
            "강화할 카드",
            upgradableCards,
            true
        );
    }

    // =====================================================
    // 팝업 닫기
    // =====================================================

    public void ClosePopup()
    {
        if (popupCanvasGroup == null)
            return;

        if (!isOpen &&
            !isClosing &&
            popupCanvasGroup.alpha <= 0.001f &&
            transitionCoroutine == null)
        {
            return;
        }

        isOpen = false;
        isClosing = true;

        requestedPopupId = null;
        requestedCards.Clear();

        ResetUpgradeSelection();

        StartTransition(CloseRoutine());
    }

    // =====================================================
    // 카드 목록 표시 요청
    // =====================================================

    private void ShowCards(
        string popupId,
        string title,
        IList<CardInstance> cards,
        bool allowCardClick)
    {
        if (popupRoot == null ||
            popupCanvasGroup == null ||
            contentRoot == null ||
            cardSelectionPrefab == null)
        {
            Debug.LogError(
                "[CardPopupManager] Inspector 참조가 설정되지 않았습니다.",
                this
            );

            return;
        }

        List<CardInstance> cardSnapshot =
            new List<CardInstance>();

        if (cards != null)
        {
            foreach (CardInstance card in cards)
            {
                if (card != null && card.data != null)
                {
                    cardSnapshot.Add(card);
                }
            }
        }

        bool sameRequestedContent =
            requestedPopupId == popupId &&
            requestedAllowCardClick == allowCardClick &&
            AreSameCards(requestedCards, cardSnapshot);

        bool popupHasContent =
            isOpen ||
            isClosing ||
            transitionCoroutine != null ||
            popupCanvasGroup.alpha > 0.001f;

        if (sameRequestedContent &&
            popupHasContent &&
            !isClosing)
        {
            return;
        }

        requestedPopupId = popupId;
        requestedAllowCardClick = allowCardClick;

        requestedCards.Clear();
        requestedCards.AddRange(cardSnapshot);

        StartTransition(
            ShowCardsRoutine(
                popupId,
                title,
                cardSnapshot,
                allowCardClick
            )
        );
    }

    // =====================================================
    // 팝업 열기 및 내용 교체
    // =====================================================

    private IEnumerator ShowCardsRoutine(
        string popupId,
        string title,
        List<CardInstance> cards,
        bool allowCardClick)
    {
        bool wasVisible =
            isOpen || popupCanvasGroup.alpha > 0.001f;

        isClosing = false;

        if (wasVisible)
        {
            popupCanvasGroup.interactable = false;
            popupCanvasGroup.blocksRaycasts = false;

            yield return FadeTo(0f);
        }
        else
        {
            popupRoot.SetActive(true);
            popupCanvasGroup.alpha = 0f;
        }

        ResetUpgradeSelection();
        ClearCards();

        bool isUpgradePopup = popupId == "UpgradableDeck";

        if (upgradeSelectedButton != null)
        {
            upgradeSelectedButton.interactable = false;
            upgradeSelectedButton.gameObject.SetActive(
                isUpgradePopup
            );
        }

        if (popupTitleText != null)
        {
            popupTitleText.text = title;
        }

        foreach (CardInstance card in cards)
        {
            GameObject cardObject = Instantiate(
                cardSelectionPrefab,
                contentRoot
            );

            spawnedCards.Add(cardObject);

            CardSelectionUI selectionUI =
                cardObject.GetComponent<CardSelectionUI>();

            if (selectionUI == null)
            {
                Debug.LogError(
                    "[CardPopupManager] Card Selection 프리팹에 " +
                    "CardSelectionUI가 없습니다.",
                    cardObject
                );

                cardObject.SetActive(false);
                continue;
            }

            if (isUpgradePopup)
            {
                selectionUI.SetupForUpgrade(
                    card,
                    HandleUpgradeCardClicked
                );
            }
            else
            {
                selectionUI.Setup(card, null);
                selectionUI.SetClickEnabled(false);
            }
        }

        Canvas.ForceUpdateCanvases();

        RectTransform contentRect =
            contentRoot as RectTransform;

        if (contentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                contentRect
            );
        }

        displayedPopupId = popupId;
        displayedAllowCardClick = allowCardClick;

        displayedCards.Clear();
        displayedCards.AddRange(cards);

        popupCanvasGroup.blocksRaycasts = true;
        popupCanvasGroup.interactable = true;

        isOpen = true;

        yield return FadeTo(1f);

        isOpen = true;
        isClosing = false;

        transitionCoroutine = null;
    }

    // =====================================================
    // 팝업 닫기 애니메이션
    // =====================================================

    private IEnumerator CloseRoutine()
    {
        if (popupCanvasGroup == null)
            yield break;

        popupCanvasGroup.interactable = false;

        yield return FadeTo(0f);

        popupCanvasGroup.blocksRaycasts = false;
        popupCanvasGroup.interactable = false;

        ClearCards();

        displayedPopupId = null;
        displayedAllowCardClick = false;
        displayedCards.Clear();

        if (upgradeSelectedButton != null)
        {
            upgradeSelectedButton.interactable = false;
            upgradeSelectedButton.gameObject.SetActive(false);
        }

        isOpen = false;
        isClosing = false;

        transitionCoroutine = null;
    }

    // =====================================================
    // 전환 관리
    // =====================================================

    private void StartTransition(IEnumerator routine)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        if (fadeTween != null)
        {
            fadeTween.Kill();
            fadeTween = null;
        }

        transitionCoroutine = StartCoroutine(routine);
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (popupCanvasGroup == null)
            yield break;

        fadeTween?.Kill();

        fadeTween = popupCanvasGroup
            .DOFade(
                targetAlpha,
                Mathf.Max(0.01f, fadeDuration)
            )
            .SetEase(Ease.Linear)
            .SetUpdate(true);

        yield return fadeTween.WaitForCompletion();

        fadeTween = null;
    }

    // =====================================================
    // 카드 생성 및 정리
    // =====================================================

    private void ClearCards()
    {
        foreach (GameObject card in spawnedCards)
        {
            if (card != null)
            {
                Destroy(card);
            }
        }

        spawnedCards.Clear();
    }

    // =====================================================
    // 카드 목록 비교
    // =====================================================

    private bool AreSameCards(
        IList<CardInstance> first,
        IList<CardInstance> second)
    {
        if (first.Count != second.Count)
            return false;

        for (int i = 0; i < first.Count; i++)
        {
            if (!ReferenceEquals(first[i], second[i]))
            {
                return false;
            }
        }

        return true;
    }

    // =====================================================
    // 뽑을 카드 더미 표시 순서 캐싱
    // =====================================================

    private void RefreshCachedDrawOrder(
        List<CardInstance> currentDrawPile)
    {
        bool sameCards =
            cachedDrawOrder.Count == currentDrawPile.Count;

        if (sameCards)
        {
            foreach (CardInstance card in cachedDrawOrder)
            {
                if (card == null ||
                    !currentDrawPile.Contains(card))
                {
                    sameCards = false;
                    break;
                }
            }
        }

        if (sameCards)
            return;

        cachedDrawOrder.Clear();
        cachedDrawOrder.AddRange(currentDrawPile);

        for (int i = cachedDrawOrder.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);

            CardInstance temp = cachedDrawOrder[i];

            cachedDrawOrder[i] = cachedDrawOrder[j];
            cachedDrawOrder[j] = temp;
        }
    }

    // =====================================================
    // 실제 뽑을 카드 더미 셔플 알림
    // =====================================================

    public void OnPlayerDrawPileShuffled()
    {
        Entity player = GetPlayer();

        if (player == null)
            return;

        cachedDrawOrder.Clear();

        foreach (CardInstance card in player.drawPile)
        {
            if (card != null && card.data != null)
            {
                cachedDrawOrder.Add(card);
            }
        }

        ShuffleCardList(cachedDrawOrder);

        if (isOpen &&
            !isClosing &&
            displayedPopupId == "DrawPile")
        {
            RefreshOpenDrawPileVisualOrder();
        }
    }

    private void RefreshOpenDrawPileVisualOrder()
    {
        for (int i = 0; i < cachedDrawOrder.Count; i++)
        {
            CardInstance targetCard = cachedDrawOrder[i];

            int foundIndex = -1;

            for (int j = i; j < spawnedCards.Count; j++)
            {
                if (spawnedCards[j] == null)
                    continue;

                CardUI cardUI =
                    spawnedCards[j].GetComponentInChildren<CardUI>();

                if (cardUI != null &&
                    cardUI.GetCard() == targetCard)
                {
                    foundIndex = j;
                    break;
                }
            }

            if (foundIndex < 0)
                continue;

            GameObject targetObject = spawnedCards[foundIndex];

            spawnedCards.RemoveAt(foundIndex);
            spawnedCards.Insert(i, targetObject);

            targetObject.transform.SetSiblingIndex(i);
        }

        displayedCards.Clear();
        displayedCards.AddRange(cachedDrawOrder);

        requestedCards.Clear();
        requestedCards.AddRange(cachedDrawOrder);

        Canvas.ForceUpdateCanvases();

        RectTransform contentRect =
            contentRoot as RectTransform;

        if (contentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                contentRect
            );
        }
    }

    private void ShuffleCardList(List<CardInstance> cards)
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);

            CardInstance temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }

    // =====================================================
    // 강화 카드 선택
    // =====================================================

    private void HandleUpgradeCardClicked(
        CardSelectionUI clickedUI,
        CardInstance clickedCard)
    {
        if (displayedPopupId != "UpgradableDeck")
            return;

        if (clickedCard == null)
        {
            if (selectedUpgradeUI == clickedUI)
            {
                selectedUpgradeUI = null;
                selectedUpgradeCard = null;
            }

            UpdateUpgradeButton();
            return;
        }

        if (selectedUpgradeUI != null &&
            selectedUpgradeUI != clickedUI)
        {
            selectedUpgradeUI.SetUpgradeSelected(false);
        }

        selectedUpgradeUI = clickedUI;
        selectedUpgradeCard = clickedCard;

        if (clickedCard.data != null)
        {
            OnUpgradeCardSelected?.Invoke(clickedCard.data);
        }

        UpdateUpgradeButton();
    }

    // =====================================================
    // 강화 버튼 활성화 상태
    // =====================================================

    private void UpdateUpgradeButton()
    {
        if (upgradeSelectedButton == null)
            return;

        bool canUpgrade =
            displayedPopupId == "UpgradableDeck" &&
            selectedUpgradeCard != null &&
            selectedUpgradeCard.data != null &&
            !selectedUpgradeCard.upgraded;

        upgradeSelectedButton.interactable = canUpgrade;
    }

    // =====================================================
    // 선택한 카드 강화
    // =====================================================

    private void UpgradeSelectedCard()
    {
        if (displayedPopupId != "UpgradableDeck")
            return;

        if (selectedUpgradeCard == null ||
            selectedUpgradeCard.data == null)
        {
            UpdateUpgradeButton();
            return;
        }

        Entity player = GetPlayer();

        if (player == null)
            return;

        // 실제 플레이어 덱에 존재하는 카드인지 확인
        if (!player.deck.Contains(selectedUpgradeCard))
        {
            Debug.LogWarning(
                "[CardPopupManager] 선택한 카드가 플레이어 덱에 없습니다.",
                this
            );

            ResetUpgradeSelection();
            return;
        }

        // 이미 강화된 카드인지 재확인
        if (selectedUpgradeCard.upgraded)
        {
            ResetUpgradeSelection();
            return;
        }

        // 실제 덱에 있는 CardInstance를 강화한다.
        bool upgradeSucceeded = selectedUpgradeCard.Upgrade();

        if (!upgradeSucceeded)
        {
            ResetUpgradeSelection();
            return;
        }

        CardInstance upgradedCard = selectedUpgradeCard;

        ResetUpgradeSelection();

        if (GameData.instance != null)
        {
            GameData.instance.UpdatePlayerPileCount();
        }

        // 팝업을 닫고 완료 이벤트를 발생시킨다.
        ClosePopup();

        OnUpgradeCompleted?.Invoke(upgradedCard);
    }

    // =====================================================
    // 강화 선택 초기화
    // =====================================================

    private void ResetUpgradeSelection()
    {
        selectedUpgradeUI = null;
        selectedUpgradeCard = null;

        if (upgradeSelectedButton != null)
        {
            upgradeSelectedButton.interactable = false;
        }
    }

    // =====================================================
    // 플레이어 참조
    // =====================================================

    private Entity GetPlayer()
    {
        if (GameData.instance == null)
        {
            Debug.LogError(
                "[CardPopupManager] GameData.instance가 없습니다.",
                this
            );

            return null;
        }

        if (GameData.instance.player == null)
        {
            Debug.LogError(
                "[CardPopupManager] 플레이어 Entity가 없습니다.",
                this
            );

            return null;
        }

        return GameData.instance.player;
    }
}