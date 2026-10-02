using System.Collections;
using TMPro;
using UnityEngine;

public class PersistentUI : MonoBehaviour
{
    private static PersistentUI instance;


    [Header("HP")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Transform hpTextTransform;


    [Header("Gold")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private Transform goldTextTransform;


    [Header("Deck")]
    [SerializeField] private TMP_Text deckCountText;
    [SerializeField] private Transform deckCountTransform;


    private Entity player;


    // 현재 UI에 표시하고 있는 값
    private int currentHP = -1;
    private int currentGold = -1;
    private int currentDeckCount = -1;


    // Punch Animation Coroutine
    private Coroutine hpPunchRoutine;
    private Coroutine goldPunchRoutine;
    private Coroutine deckPunchRoutine;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);
    }


    private void Start()
    {
        BindPlayer();

        UpdateAll(false);
    }


    private void Update()
    {
        if (GameData.instance == null)
            return;


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

        if (hpPunchRoutine != null)
            StopCoroutine(hpPunchRoutine);

        if (goldPunchRoutine != null)
            StopCoroutine(goldPunchRoutine);

        if (deckPunchRoutine != null)
            StopCoroutine(deckPunchRoutine);


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


        Entity newPlayer =
            GameData.instance.player;


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


        UpdateHP(
            player.CurrentHP,
            playAnimation
        );


        UpdateGold(
            player.Gold,
            playAnimation
        );


        UpdateDeckCount(
            player.deck.Count,
            playAnimation
        );
    }


    // =====================================================
    // 데이터 변경 확인
    // =====================================================

    private void CheckPlayerData()
    {
        if (player == null)
            return;


        // -----------------------------------------
        // HP
        // -----------------------------------------

        if (currentHP != player.CurrentHP)
        {
            UpdateHP(
                player.CurrentHP,
                true
            );
        }


        // -----------------------------------------
        // Gold
        // -----------------------------------------

        if (currentGold != player.Gold)
        {
            UpdateGold(
                player.Gold,
                true
            );
        }


        // -----------------------------------------
        // Deck
        // -----------------------------------------

        int actualDeckCount =
            player.deck.Count;


        if (currentDeckCount != actualDeckCount)
        {
            UpdateDeckCount(
                actualDeckCount,
                true
            );
        }
    }


    // =====================================================
    // HP
    // =====================================================

    private void UpdateHP(int value)
    {
        UpdateHP(
            value,
            true
        );
    }


    private void UpdateHP(
        int value,
        bool playAnimation)
    {
        if (hpText == null)
            return;


        hpText.text =
            value.ToString();


        currentHP = value;


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
        UpdateGold(
            value,
            true
        );
    }


    private void UpdateGold(
        int value,
        bool playAnimation)
    {
        if (goldText == null)
            return;


        goldText.text =
            value.ToString();


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
        UpdateDeckCount(
            value,
            true
        );
    }


    private void UpdateDeckCount(
        int value,
        bool playAnimation)
    {
        if (deckCountText == null)
            return;


        deckCountText.text =
            value.ToString();


        currentDeckCount = value;


        if (playAnimation)
        {
            PlayDeckAnimation();
        }
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


        hpPunchRoutine =
            StartCoroutine(
                PunchRoutine(
                    hpTextTransform
                )
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


        goldPunchRoutine =
            StartCoroutine(
                PunchRoutine(
                    goldTextTransform
                )
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


        deckPunchRoutine =
            StartCoroutine(
                PunchRoutine(
                    deckCountTransform
                )
            );
    }


    // =====================================================
    // Punch Animation
    // =====================================================

    private IEnumerator PunchRoutine(
        Transform target)
    {
        if (target == null)
            yield break;


        Vector3 originalScale =
            Vector3.one;

        Vector3 enlargedScale =
            Vector3.one * 1.1f;


        float duration = 0.08f;
        float t = 0f;


        // -----------------------------------------
        // 커짐
        // -----------------------------------------

        while (t < duration)
        {
            if (target == null)
                yield break;


            t += Time.deltaTime;


            target.localScale =
                Vector3.Lerp(
                    originalScale,
                    enlargedScale,
                    t / duration
                );


            yield return null;
        }


        t = 0f;


        // -----------------------------------------
        // 원래 크기로
        // -----------------------------------------

        while (t < duration)
        {
            if (target == null)
                yield break;


            t += Time.deltaTime;


            target.localScale =
                Vector3.Lerp(
                    enlargedScale,
                    originalScale,
                    t / duration
                );


            yield return null;
        }


        if (target != null)
        {
            target.localScale =
                originalScale;
        }
    }
}