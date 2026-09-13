using UnityEngine;
using UnityEngine.UI;

public class MapConnectionUI : MonoBehaviour
{
    [SerializeField] private RectTransform connectionLayer;
    [SerializeField] private float lineThickness = 5f;

    public void CreateConnection(MapNode fromNode, MapNode toNode)
    {
        GameObject lineObject = new GameObject("Connection");

        lineObject.transform.SetParent(connectionLayer, false);

        Image image = lineObject.AddComponent<Image>();
        image.color = Color.white;

        RectTransform rectTransform =
            lineObject.GetComponent<RectTransform>();

        Vector2 start = fromNode.position;
        Vector2 end = toNode.position;

        Vector2 direction = end - start;

        float length = direction.magnitude;

        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rectTransform.anchorMin =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchorMax =
            new Vector2(0.5f, 0.5f);

        rectTransform.pivot =
            new Vector2(0f, 0.5f);

        rectTransform.anchoredPosition =
            start;

        rectTransform.sizeDelta =
            new Vector2(length, lineThickness);

        rectTransform.localRotation =
            Quaternion.Euler(0f, 0f, angle);
    }
}