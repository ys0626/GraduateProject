using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventChoiceUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text choiceText;

    private int choiceIndex;
    private EventPanelUI eventPanel;

    // 선택지 초기화
    public void Setup(EventChoiceData choiceData, int index, EventPanelUI panel)
    {
        choiceIndex = index;
        eventPanel = panel;

        choiceText.text = choiceData.ChoiceText;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClickChoice);
    }

    // 버튼 클릭
    private void OnClickChoice()
    {
        eventPanel.SelectChoice(choiceIndex);
    }

    // 버튼 활성화 여부
    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;
    }
}