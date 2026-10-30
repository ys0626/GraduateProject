
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapNodeUI : MonoBehaviour
{
    [Header("Node UI")]
    [SerializeField] private Image background;

    [Header("Node Sprites")]
    [SerializeField] private Sprite startSprite;
    [SerializeField] private Sprite combatSprite;
    [SerializeField] private Sprite eliteSprite;
    [SerializeField] private Sprite eventSprite;
    [SerializeField] private Sprite shopSprite;
    [SerializeField] private Sprite restSprite;
    [SerializeField] private Sprite bossSprite;

    [Header("Scale Animation")]
    [SerializeField] private float scaleMultiplier = 1.15f;
    [SerializeField] private float animationDuration = 0.7f;
    [SerializeField] private float animationStartDelay = 0.3f;



    [Header("Node Appearance")]
    [SerializeField] private Color visitedColor = new Color(0.7f, 0.7f, 0.7f, 1f);

    private MapNode node;
    private Button button;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    private TooltipTrigger tooltipTrigger;

    private void Awake()
    {
        button = GetComponent<Button>();
        tooltipTrigger = GetComponent<TooltipTrigger>();
        originalScale = transform.localScale;
    }

    public void Setup(MapNode mapNode)
    {
        node = mapNode;

        SetNodeSprite(mapNode.type);

        SetupTooltip(mapNode.type);

        if (background != null)
        {
            background.enabled = true;
            background.color = mapNode.visited ? visitedColor : Color.white;
        }

        originalScale = transform.localScale;
        transform.localScale = originalScale;

        SetInteractable(false);
    }

    public void SetInteractable(bool value)
    {
        if (button != null)
            button.interactable = value;

        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }

        transform.localScale = originalScale;

        if (value)
            scaleCoroutine = StartCoroutine(ScaleAnimation());
    }

    private IEnumerator ScaleAnimation()
    {
        float randomDelay = UnityEngine.Random.Range(0f, animationStartDelay);
        yield return new WaitForSecondsRealtime(randomDelay);

        while (true)
        {
            yield return StartCoroutine(
                ScaleTo(originalScale * scaleMultiplier)
            );

            yield return StartCoroutine(
                ScaleTo(originalScale)
            );
        }
    }

    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, animationDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            transform.localScale = Vector3.Lerp(
                startScale,
                targetScale,
                progress
            );

            yield return null;
        }

        transform.localScale = targetScale;
    }

    private void SetNodeSprite(MapNodeType type)
    {
        Sprite selectedSprite = null;

        switch (type)
        {
            case MapNodeType.Start:
                selectedSprite = startSprite;
                break;

            case MapNodeType.Combat:
                selectedSprite = combatSprite;
                break;

            case MapNodeType.Elite:
                selectedSprite = eliteSprite;
                break;

            case MapNodeType.Event:
                selectedSprite = eventSprite;
                break;

            case MapNodeType.Shop:
                selectedSprite = shopSprite;
                break;

            case MapNodeType.Rest:
                selectedSprite = restSprite;
                break;

            case MapNodeType.Boss:
                selectedSprite = bossSprite;
                break;
        }

        if (background == null)
        {
            Debug.LogError(
                "MapNodeUI: Background Image가 할당되지 않았습니다.",
                this
            );
            return;
        }

        if (selectedSprite == null)
        {
            Debug.LogError(
                $"MapNodeUI: {type} 노드의 스프라이트가 할당되지 않았습니다.",
                this
            );
            return;
        }

        background.sprite = selectedSprite;
    }

    private void SetupTooltip(MapNodeType type)
    {
        if (tooltipTrigger == null)
            return;

        string title = "";
        string description = "";

        switch (type)
        {
            case MapNodeType.Start:
                title = "시작 지점";
                description = "모험이 시작되는 지점입니다.";
                break;

            case MapNodeType.Combat:
                title = "일반 전투";
                description = "적과 전투를 진행합니다.";
                break;

            case MapNodeType.Elite:
                title = "정예 전투";
                description = "강력한 정예 적과 전투합니다. 특별한 보상을 얻을 수 있습니다.";
                break;

            case MapNodeType.Event:
                title = "이벤트";
                description = "다양한 사건을 만나며 선택에 따라 결과가 달라집니다.";
                break;

            case MapNodeType.Shop:
                title = "상점";
                description = "골드를 사용하여 카드와 아이템을 구매할 수 있습니다.";
                break;

            case MapNodeType.Rest:
                title = "휴식";
                description = "체력을 회복하거나 덱의 카드를 강화할 수 있습니다.";
                break;

            case MapNodeType.Boss:
                title = "보스 전투";
                description = "강력한 보스와 최종 전투를 진행합니다.";
                break;
        }

        tooltipTrigger.SetContent(title, description);
    }
}
