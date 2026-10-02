using System.Collections;
using TMPro;
using UnityEngine;

public class PersistentUIManager : MonoBehaviour
{
    public static PersistentUIManager instance;

    [Header("HP")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Transform hpTransform;

    [Header("Gold")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private Transform goldTransform;

    [Header("Deck Count")]
    [SerializeField] private TMP_Text deckCountText;
    [SerializeField] private Transform deckCountTransform;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        RegisterEvents();
        UpdateAll();
    }


    private void OnDestroy()
    {
        UnregisterEvents();
    }


    // =====================================================
    // 이벤트 등록
    // =====================================================

    private void RegisterEvents()
    {
        if (GameData.instance == null)
            return;

        if (GameData.instance.player == null)
            return;


        // 플레이어 HP
        GameData.instance.player.OnCurrentHPChanged += UpdatePlayerHP;
        GameData.instance.player.OnMaxHPChanged += UpdatePlayerHP;

        // 플레이어 Gold
        GameData.instance.player.OnGoldChanged += UpdateGold;

        // 플레이어 Deck Count
        GameData.instance.player.OnDeckCountChanged += UpdatePlayerDeckCount;
    }


    // =====================================================
    // 이벤트 해제
    // =====================================================

    private void UnregisterEvents()
    {
        if (GameData.instance == null)
            return;

        if (GameData.instance.player == null)
            return;


        // 플레이어 HP
        GameData.instance.player.OnCurrentHPChanged -= UpdatePlayerHP;
        GameData.instance.player.OnMaxHPChanged -= UpdatePlayerHP;

        // 플레이어 Gold
        GameData.instance.player.OnGoldChanged -= UpdateGold;

        // 플레이어 Deck Count
        GameData.instance.player.OnDeckCountChanged -= UpdatePlayerDeckCount;
    }


    // =====================================================
    // 모든 UI 갱신
    // =====================================================

    public void UpdateAll()
    {
        if (GameData.instance == null)
            return;

        if (GameData.instance.player == null)
            return;


        UpdatePlayerHP(0);

        UpdateGold(
            GameData.instance.player.Gold
        );

        UpdatePlayerDeckCount(
            GameData.instance.player.DeckCount
        );
    }


    // =====================================================
    // 플레이어 HP
    // =====================================================

    private void UpdatePlayerHP(int _)
    {
        if (hpText == null)
            return;


        hpText.text =
            GameData.instance.player.CurrentHP +
            "/" +
            GameData.instance.player.MaxHP;


        PlayPunchAnimation(hpTransform);
    }


    // =====================================================
    // Gold
    // =====================================================

    private void UpdateGold(int value)
    {
        if (goldText == null)
            return;


        goldText.text = value.ToString();

        PlayPunchAnimation(goldTransform);
    }


    // =====================================================
    // Deck Count
    // =====================================================

    private void UpdatePlayerDeckCount(int value)
    {
        if (deckCountText == null)
            return;


        deckCountText.text = value.ToString();

        PlayPunchAnimation(deckCountTransform);
    }


    // =====================================================
    // Animation
    // =====================================================

    public void PlayPunchAnimation(Transform target)
    {
        if (target == null)
            return;

        StartCoroutine(PunchRoutine(target));
    }


    private IEnumerator PunchRoutine(Transform target)
    {
        if (target == null)
            yield break;


        Vector3 originalScale = Vector3.one;
        Vector3 enlargedScale = Vector3.one * 1.1f;

        float duration = 0.08f;
        float t = 0f;


        // 커짐
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


        // 원래 크기로
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
            target.localScale = originalScale;
    }
}