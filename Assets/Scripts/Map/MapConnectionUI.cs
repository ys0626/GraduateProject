
using UnityEngine;
using UnityEngine.UI;

public class MapConnectionUI : MonoBehaviour
{
    [SerializeField] private RectTransform connectionLayer;

    [Header("Dashed Line Settings")]
    [SerializeField] private float lineThickness = 4f;
    [SerializeField] private float dashLength = 10f;
    [SerializeField] private float dashSpacing = 6f;
    [SerializeField] private Color lineColor = Color.black;

    public void CreateConnection(MapNode fromNode, MapNode toNode)
    {
        Vector2 start = fromNode.position;
        Vector2 end = toNode.position;

        Vector2 direction = end - start;
        float length = direction.magnitude;

        if (length <= 0f)
            return;

        direction.Normalize();

        float angle = Mathf.Atan2(direction.y, direction.x)
                      * Mathf.Rad2Deg;

        float step = dashLength + dashSpacing;

        for (float distance = 0f; distance < length; distance += step)
        {
            float currentDashLength =
                Mathf.Min(dashLength, length - distance);

            GameObject dashObject = new GameObject("ConnectionDash");
            dashObject.transform.SetParent(connectionLayer, false);

            Image image = dashObject.AddComponent<Image>();
            image.color = lineColor;
            image.raycastTarget = false;

            RectTransform rectTransform =
                dashObject.GetComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0f, 0.5f);

            rectTransform.anchoredPosition = start + direction * distance;
            rectTransform.sizeDelta =
                new Vector2(currentDashLength, lineThickness);

            rectTransform.localRotation =
                Quaternion.Euler(0f, 0f, angle);
        }
    }
}
