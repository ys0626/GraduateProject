
using UnityEngine;
using UnityEngine.UI;

public class CardPileButton : MonoBehaviour
{
    public enum PileType
    {
        Draw,
        Discard,
        Exhaust
    }

    [SerializeField] private PileType pileType;
    [SerializeField] private Button button;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(OpenCardList);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OpenCardList);
    }

    private void OpenCardList()
    {
        if (CardPopupManager.instance == null)
        {
            Debug.LogError("CardPopupManager 인스턴스를 찾을 수 없습니다.");
            return;
        }

        switch (pileType)
        {
            case PileType.Draw:
                CardPopupManager.instance.OpenDrawPile();
                break;

            case PileType.Discard:
                CardPopupManager.instance.OpenDiscardPile();
                break;

            case PileType.Exhaust:
                CardPopupManager.instance.OpenExhaustPile();
                break;
        }
    }
}
