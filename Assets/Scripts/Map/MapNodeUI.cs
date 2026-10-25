
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

    private void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;
    }

    public void Setup(MapNode mapNode)
    {
        node = mapNode;

        SetNodeSprite(mapNode.type);

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
}
