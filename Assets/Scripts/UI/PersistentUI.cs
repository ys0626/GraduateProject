
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PersistentUI : MonoBehaviour
{
    private static PersistentUI instance;
    public static PersistentUI Instance => instance;

    [Header("HP")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Transform hpTextTransform;

    [Header("Gold")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private Transform goldTextTransform;

    [Header("Cheese Inventory")]
    [SerializeField] private Button[] cheeseSlotButtons;
    [SerializeField] private Image[] cheeseSlotIcons;
    [SerializeField] private Sprite emptyCheeseSlotIcon;

    [Header("Cheese Full Notice")]
    [SerializeField] private GameObject cheeseFullPopup;
    [SerializeField] private TMP_Text cheeseFullPopupText;

    [SerializeField] private float cheeseFullNoticeDuration = 2f;

    [Header("Deck")]
    [SerializeField] private TMP_Text deckCountText;
    [SerializeField] private Transform deckCountTransform;

    private Entity player;
    private GameData subscribedGameData;

    // 현재 UI에 표시하고 있는 값
    private int currentHP = -1;
    private int currentMaxHP = -1;
    private int currentGold = -1;
    private int currentDeckCount = -1;

    // Punch Animation Coroutine
    private Coroutine hpPunchRoutine;
    private Coroutine goldPunchRoutine;
    private Coroutine deckPunchRoutine;

    // 치즈 알림 Coroutine
    private Coroutine cheeseFullPopupCoroutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);

        if (cheeseFullPopup != null)
        {
            cheeseFullPopup.SetActive(false);
        }
    }

    private void Start()
    {
        SetupCheeseSlots();

        BindPlayer();
        BindCheeseInventoryEvents();

        UpdateAll(false);
        RefreshCheeseSlots();
    }

    private void Update()
    {
        if (GameData.instance == null)
            return;

        // GameData가 나중에 초기화되거나 교체된 경우 연결 갱신
        if (subscribedGameData != GameData.instance)
        {
            BindPlayer();
            BindCheeseInventoryEvents();
            RefreshCheeseSlots();
        }

        if (player == null)
        {
            BindPlayer();
            UpdateAll(false);
            return;
        }

        CheckPlayerData();
    }

    private void OnDestroy()
    {
        UnregisterEvents();
        UnregisterCheeseInventoryEvents();

        if (hpPunchRoutine != null)
            StopCoroutine(hpPunchRoutine);

        if (goldPunchRoutine != null)
            StopCoroutine(goldPunchRoutine);

        if (deckPunchRoutine != null)
            StopCoroutine(deckPunchRoutine);

        if (cheeseFullPopupCoroutine != null)
            StopCoroutine(cheeseFullPopupCoroutine);

        if (instance == this)
        {
            instance = null;
        }
    }

    // =====================================================
    // Player 연결
    // =====================================================

    private void BindPlayer()
    {
        if (GameData.instance == null)
            return;

        Entity newPlayer = GameData.instance.player;

        if (newPlayer == null)
            return;

        if (player == newPlayer)
            return;

        UnregisterEvents();

        player = newPlayer;

        // HP
        player.OnCurrentHPChanged += UpdateHP;
        player.OnMaxHPChanged += UpdateHP;

        // Gold
        player.OnGoldChanged += UpdateGold;

        // Deck
        player.OnDeckCountChanged += UpdateDeckCount;
    }

    private void UnregisterEvents()
    {
        if (player == null)
            return;

        // HP
        player.OnCurrentHPChanged -= UpdateHP;
        player.OnMaxHPChanged -= UpdateHP;

        // Gold
        player.OnGoldChanged -= UpdateGold;

        // Deck
        player.OnDeckCountChanged -= UpdateDeckCount;

        player = null;
    }

    // =====================================================
    // 치즈 인벤토리 이벤트 연결
    // =====================================================

    private void BindCheeseInventoryEvents()
    {
        GameData newGameData = GameData.instance;

        if (subscribedGameData == newGameData)
            return;

        UnregisterCheeseInventoryEvents();

        if (newGameData == null)
            return;

        subscribedGameData = newGameData;
        subscribedGameData.OnCheeseInventoryChanged += RefreshCheeseSlots;
    }

    private void UnregisterCheeseInventoryEvents()
    {
        if (subscribedGameData == null)
            return;

        subscribedGameData.OnCheeseInventoryChanged -= RefreshCheeseSlots;
        subscribedGameData = null;
    }

    // =====================================================
    // 모든 UI 갱신
    // =====================================================

    public void UpdateAll(bool playAnimation)
    {
        if (GameData.instance == null)
            return;

        if (player == null)
        {
            BindPlayer();
        }

        if (player == null)
            return;

        UpdateHP(player.CurrentHP, playAnimation);
        UpdateGold(player.Gold, playAnimation);
        UpdateDeckCount(player.deck.Count, playAnimation);
    }

    // =====================================================
    // 데이터 변경 확인
    // =====================================================

    private void CheckPlayerData()
    {
        if (player == null)
            return;

        // HP
        if (currentHP != player.CurrentHP ||
            currentMaxHP != player.MaxHP)
        {
            UpdateHP(player.CurrentHP, true);
        }

        // Gold
        if (currentGold != player.Gold)
        {
            UpdateGold(player.Gold, true);
        }

        // Deck
        int actualDeckCount = player.deck.Count;

        if (currentDeckCount != actualDeckCount)
        {
            UpdateDeckCount(actualDeckCount, true);
        }
    }

    // =====================================================
    // HP
    // =====================================================

    private void UpdateHP(int value)
    {
        UpdateHP(value, true);
    }

    private void UpdateHP(int value, bool playAnimation)
    {
        if (hpText == null || player == null)
            return;

        hpText.text = player.CurrentHP + "/" + player.MaxHP;

        currentHP = player.CurrentHP;
        currentMaxHP = player.MaxHP;

        if (playAnimation)
        {
            PlayHPAnimation();
        }
    }

    // =====================================================
    // Gold
    // =====================================================

    private void UpdateGold(int value)
    {
        UpdateGold(value, true);
    }

    private void UpdateGold(int value, bool playAnimation)
    {
        if (goldText == null)
            return;

        goldText.text = value.ToString();
        currentGold = value;

        if (playAnimation)
        {
            PlayGoldAnimation();
        }
    }

    // =====================================================
    // Deck
    // =====================================================

    private void UpdateDeckCount(int value)
    {
        UpdateDeckCount(value, true);
    }

    private void UpdateDeckCount(int value, bool playAnimation)
    {
        if (deckCountText == null)
            return;

        deckCountText.text = value.ToString();
        currentDeckCount = value;

        if (playAnimation)
        {
            PlayDeckAnimation();
        }
    }

    // =====================================================
    // 치즈 슬롯 초기화
    // =====================================================

    private void SetupCheeseSlots()
    {
        if (cheeseSlotButtons == null)
            return;

        for (int i = 0; i < cheeseSlotButtons.Length; i++)
        {
            int slotIndex = i;

            if (cheeseSlotButtons[i] == null)
                continue;

            cheeseSlotButtons[i].onClick.RemoveAllListeners();

            cheeseSlotButtons[i].onClick.AddListener(
                () => OnCheeseSlotClicked(slotIndex)
            );
        }
    }

    private void OnCheeseSlotClicked(int slotIndex)
    {
        if (GameData.instance == null)
            return;

        bool used = GameData.instance.UseCheese(slotIndex);

        if (used)
        {
            RefreshCheeseSlots();
            UpdateAll(false);
        }
    }

    public void RefreshCheeseSlots()
    {
        if (GameData.instance == null ||
            cheeseSlotButtons == null)
        {
            return;
        }

        CheeseData[] inventory = GameData.instance.CheeseInventory;

        for (int i = 0; i < cheeseSlotButtons.Length; i++)
        {
            CheeseData cheese = null;

            if (inventory != null && i < inventory.Length)
            {
                cheese = inventory[i];
            }

            if (cheeseSlotButtons[i] != null)
            {
                // 빈 슬롯은 클릭 불가
                cheeseSlotButtons[i].interactable = cheese != null;
            }

            Image icon = null;

            if (cheeseSlotIcons != null &&
                i < cheeseSlotIcons.Length)
            {
                icon = cheeseSlotIcons[i];
            }

            if (icon == null)
                continue;

            if (cheese != null && cheese.icon != null)
            {
                // 치즈가 있으면 해당 치즈 그림 표시
                icon.sprite = cheese.icon;
                icon.enabled = true;
            }
            else
            {
                // 치즈가 없으면 빈 슬롯 그림 표시
                icon.sprite = emptyCheeseSlotIcon;
                icon.enabled = emptyCheeseSlotIcon != null;
            }
        }
    }

    // =====================================================
    // 치즈 인벤토리 가득 참 알림
    // =====================================================

    public void ShowCheeseInventoryFullPopup()
    {
        if (cheeseFullPopup == null)
        {
            Debug.LogWarning(
                "Cheese Full Notice가 연결되지 않았습니다."
            );
            return;
        }

        if (cheeseFullPopupText != null)
        {
            cheeseFullPopupText.text = "더 이상 치즈를 가질 수 없어!";
        }

        // 이미 알림이 표시 중이면 시간을 다시 시작
        if (cheeseFullPopupCoroutine != null)
        {
            StopCoroutine(cheeseFullPopupCoroutine);
        }

        cheeseFullPopupCoroutine =
            StartCoroutine(ShowCheeseFullNoticeRoutine());
    }

    private IEnumerator ShowCheeseFullNoticeRoutine()
    {
        cheeseFullPopup.SetActive(true);

        yield return new WaitForSeconds(cheeseFullNoticeDuration);

        if (cheeseFullPopup != null)
        {
            cheeseFullPopup.SetActive(false);
        }

        cheeseFullPopupCoroutine = null;
    }

    // =====================================================
    // HP Animation
    // =====================================================

    private void PlayHPAnimation()
    {
        if (hpTextTransform == null)
            return;

        if (hpPunchRoutine != null)
        {
            StopCoroutine(hpPunchRoutine);
        }

        hpPunchRoutine = StartCoroutine(
            PunchRoutine(hpTextTransform)
        );
    }

    // =====================================================
    // Gold Animation
    // =====================================================

    private void PlayGoldAnimation()
    {
        if (goldTextTransform == null)
            return;

        if (goldPunchRoutine != null)
        {
            StopCoroutine(goldPunchRoutine);
        }

        goldPunchRoutine = StartCoroutine(
            PunchRoutine(goldTextTransform)
        );
    }

    // =====================================================
    // Deck Animation
    // =====================================================

    private void PlayDeckAnimation()
    {
        if (deckCountTransform == null)
            return;

        if (deckPunchRoutine != null)
        {
            StopCoroutine(deckPunchRoutine);
        }

        deckPunchRoutine = StartCoroutine(
            PunchRoutine(deckCountTransform)
        );
    }

    // =====================================================
    // Punch Animation
    // =====================================================

    private IEnumerator PunchRoutine(Transform target)
    {
        if (target == null)
            yield break;

        Vector3 originalScale = target.localScale;
        Vector3 enlargedScale = originalScale * 1.1f;

        float duration = 0.08f;
        float t = 0f;

        // 커짐
        while (t < duration)
        {
            if (target == null)
                yield break;

            t += Time.deltaTime;

            target.localScale = Vector3.Lerp(
                originalScale,
                enlargedScale,
                t / duration
            );

            yield return null;
        }

        t = 0f;

        // 원래 크기로
        while (t < duration)
        {
            if (target == null)
                yield break;

            t += Time.deltaTime;

            target.localScale = Vector3.Lerp(
                enlargedScale,
                originalScale,
                t / duration
            );

            yield return null;
        }

        if (target != null)
        {
            target.localScale = originalScale;
        }
    }
}
