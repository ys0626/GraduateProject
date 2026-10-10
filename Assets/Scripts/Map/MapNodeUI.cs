using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapNodeUI : MonoBehaviour
{
    [Header("Node UI")]
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text text;

    [Header("Available Node Highlight")]
    [SerializeField] private Outline nodeOutline;
    [SerializeField] private Color availableOutlineColor = Color.yellow;
    [SerializeField] private Color availableTextColor = Color.white;
    [SerializeField]
    private Color unavailableColor =
        new Color(0.55f, 0.55f, 0.55f, 0.65f);

    [System.NonSerialized] private MapNode node;

    private Button button;
    private Color originalBackgroundColor;
    private Color originalTextColor;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (nodeOutline == null)
        {
            nodeOutline = GetComponent<Outline>();
        }

        if (nodeOutline != null)
        {
            nodeOutline.enabled = false;
        }
    }

    public void Setup(MapNode mapNode)
    {
        node = mapNode;

        text.text = GetNodeText(mapNode.type);

        SetNodeColor(mapNode.type);

        originalBackgroundColor = background.color;
        originalTextColor = text.color;

        SetInteractable(false);
    }

    public void SetInteractable(bool value)
    {
        if (button != null)
        {
            button.interactable = value;
        }

        if (background != null)
        {
            background.color = value
                ? originalBackgroundColor
                : originalBackgroundColor * unavailableColor;
        }

        if (text != null)
        {
            text.color = value
                ? availableTextColor
                : originalTextColor * unavailableColor;
        }

        if (nodeOutline != null)
        {
            nodeOutline.enabled = value;
            nodeOutline.effectColor = availableOutlineColor;
        }
    }

    private string GetNodeText(MapNodeType type)
    {
        switch (type)
        {
            case MapNodeType.Start:
                return "START";

            case MapNodeType.Combat:
                return "COMBAT";

            case MapNodeType.Elite:
                return "ELITE";

            case MapNodeType.Event:
                return "EVENT";

            case MapNodeType.Shop:
                return "SHOP";

            case MapNodeType.Rest:
                return "REST";

            case MapNodeType.Boss:
                return "BOSS";

            default:
                return "?";
        }
    }

    private void SetNodeColor(MapNodeType type)
    {
        switch (type)
        {
            case MapNodeType.Start:
                background.color = Color.white;
                break;

            case MapNodeType.Combat:
                background.color = Color.gray;
                break;

            case MapNodeType.Elite:
                background.color = Color.red;
                break;

            case MapNodeType.Event:
                background.color = Color.yellow;
                break;

            case MapNodeType.Shop:
                background.color = Color.green;
                break;

            case MapNodeType.Rest:
                background.color = Color.cyan;
                break;

            case MapNodeType.Boss:
                background.color = Color.magenta;
                break;
        }
    }
}