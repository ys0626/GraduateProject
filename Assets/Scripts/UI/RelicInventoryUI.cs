using UnityEngine;
using UnityEngine.UI;

public class RelicInventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform relicContainer;
    [SerializeField] private GameObject relicIconPrefab;

    private RelicManager relicManager;

    private void OnEnable()
    {
        relicManager = RelicManager.EnsureInstance();

        relicManager.OnRelicsChanged += RefreshUI;

        RefreshUI();
    }

    private void OnDisable()
    {
        if (relicManager != null)
        {
            relicManager.OnRelicsChanged -= RefreshUI;
        }
    }

    private void RefreshUI()
    {
        if (relicContainer == null || relicIconPrefab == null)
        {
            Debug.LogWarning(
                "RelicInventoryUI: UI 참조가 연결되지 않았습니다.");
            return;
        }

        for (int i = relicContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(relicContainer.GetChild(i).gameObject);
        }

        if (relicManager == null)
            return;

        foreach (RelicInstance relic in relicManager.Relics)
        {
            if (relic == null || relic.Data == null)
                continue;

            GameObject iconObject = Instantiate(
                relicIconPrefab, relicContainer);

            Image iconImage = iconObject.GetComponent<Image>();

            if (iconImage != null)
            {
                iconImage.sprite = relic.Data.Icon;
                iconImage.preserveAspect = true;
            }
        }
    }
}