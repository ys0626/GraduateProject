using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 턴 시작 시 화면 중앙에 턴 정보를 출력하는 UI
/// </summary>
public class TurnBanner : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rect;
    [SerializeField] private TextMeshProUGUI turnText;

    [Header("Text Color")]
    [SerializeField] private Color playerTurnColor = new Color(0.40f, 0.80f, 0.40f, 1f);
    [SerializeField] private Color enemyTurnColor = new Color(0.90f, 0.22f, 0.21f, 1f);

    [Header("Animation")]
    private float showDuration = 0.35f;
    private float hideDuration = 0.3f;
    private float stayDuration = 1.0f;

    private float moveDistance = 40f;

    private Coroutine routine;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// 턴 배너 출력
    /// </summary>
    public void ShowTurn(
    int turnCount,
    BattleManager.BattlePhase phase)
    {
        if (routine != null)
            StopCoroutine(routine);

        bool isPlayerTurn =
            phase == BattleManager.BattlePhase.PlayerTurn;

        string phaseText = isPlayerTurn ? "Player" : "Enemy";

        Color phaseColor =
            isPlayerTurn ? playerTurnColor : enemyTurnColor;

        // Color -> HTML 색상으로 변환
        string colorHex = ColorUtility.ToHtmlStringRGB(phaseColor);

        // 한 줄 + 부분 색상 적용
        turnText.text =
            $"Turn {turnCount} <color=#{colorHex}>{phaseText}</color>";

        routine = StartCoroutine(ShowRoutine());
    }

    /// <summary>
    /// 턴 배너 애니메이션
    /// </summary>
    private IEnumerator ShowRoutine()
    {
        Vector2 originalPos = rect.anchoredPosition;

        Vector2 startPos =
            originalPos + Vector2.down * moveDistance;

        Vector3 startScale = Vector3.one * 0.7f;
        Vector3 overshootScale = Vector3.one * 1.08f;
        Vector3 endScale = Vector3.one;

        // 초기 상태
        rect.anchoredPosition = startPos;

        rect.localScale = startScale;

        canvasGroup.alpha = 0f;

        float t = 0f;

        // =====================================================
        // 등장
        // =====================================================

        while (t < showDuration)
        {
            t += Time.deltaTime;

            float normalized = t / showDuration;

            // 부드러운 감속
            float eased =
                1f - Mathf.Pow(1f - normalized, 3f);

            canvasGroup.alpha =
                Mathf.Lerp(0f, 1f, eased);

            rect.anchoredPosition =
                Vector2.Lerp(
                    startPos,
                    originalPos,
                    eased);

            rect.localScale =
                Vector3.Lerp(
                    startScale,
                    overshootScale,
                    eased);

            yield return null;
        }

        // =====================================================
        // 살짝 안정화
        // =====================================================

        t = 0f;

        while (t < 0.12f)
        {
            t += Time.deltaTime;

            float normalized = t / 0.12f;

            rect.localScale =
                Vector3.Lerp(
                    overshootScale,
                    endScale,
                    normalized);

            yield return null;
        }

        // =====================================================
        // 유지
        // =====================================================

        yield return new WaitForSeconds(stayDuration);

        // =====================================================
        // 사라짐
        // =====================================================

        t = 0f;

        while (t < hideDuration)
        {
            t += Time.deltaTime;

            float normalized = t / hideDuration;

            float eased =
                normalized * normalized;

            canvasGroup.alpha =
                Mathf.Lerp(1f, 0f, eased);

            rect.localScale =
                Vector3.Lerp(
                    endScale,
                    Vector3.one * 0.95f,
                    eased);

            yield return null;
        }

        canvasGroup.alpha = 0f;

        rect.anchoredPosition = originalPos;

        rect.localScale = Vector3.one;
    }
}