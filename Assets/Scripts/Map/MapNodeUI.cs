using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapNodeUI : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text text;

    [System.NonSerialized] private MapNode node;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Setup(MapNode mapNode)
    {
        node = mapNode;

        text.text = GetNodeText(mapNode.type);

        SetNodeColor(mapNode.type);

        SetInteractable(false);
    }

    public void SetInteractable(bool value)
    {
        button.interactable = value;
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