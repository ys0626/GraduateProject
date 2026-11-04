
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventPanelUI : MonoBehaviour
{
    [Header("Event UI")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text eventNameText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Choice UI")]
    [SerializeField] private Transform choiceContent;
    [SerializeField] private EventChoiceUI choicePrefab;

    [Header("Continue UI")]
    [SerializeField] private Button continueButton;

    private EventManager eventManager;

    private readonly List<EventChoiceUI> choiceUIs =
        new List<EventChoiceUI>();

    private void Awake()
    {
        // 처음에는 계속하기 버튼 숨기기
        continueButton.gameObject.SetActive(false);

        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(OnClickContinue);
    }

    // 이벤트 화면 초기화
    public void Setup(EventData eventData, EventManager manager)
    {
        eventManager = manager;

        backgroundImage.sprite = eventData.Background;
        eventNameText.text = eventData.EventName;
        descriptionText.text = eventData.Description;

        ClearChoices();

        // 선택지 영역 활성화
        choiceContent.gameObject.SetActive(true);

        // 계속하기 버튼 숨기기
        continueButton.gameObject.SetActive(false);

        for (int i = 0; i < eventData.Choices.Count; i++)
        {
            EventChoiceUI choiceUI =
                Instantiate(choicePrefab, choiceContent);

            choiceUI.Setup(eventData.Choices[i], i, this);

            choiceUIs.Add(choiceUI);
        }
    }

    // 선택지 클릭
    public void SelectChoice(int index)
    {
        eventManager.SelectChoice(index);
    }

    // 선택지 버튼 활성화 설정
    public void SetChoiceInteractable(int index, bool interactable)
    {
        if (index < 0 || index >= choiceUIs.Count)
            return;

        choiceUIs[index].SetInteractable(interactable);
    }

    // 선택지 결과 표시
    public void ShowResult(string resultDescription)
    {
        // 기존 이벤트 설명을 결과 설명으로 교체
        descriptionText.text = resultDescription;

        // 모든 선택지 버튼 숨기기
        choiceContent.gameObject.SetActive(false);

        // 계속하기 버튼 표시
        continueButton.gameObject.SetActive(true);
    }

    // 계속하기 버튼
    private void OnClickContinue()
    {
        eventManager.ContinueToMap();
    }

    // 기존 선택지 제거
    private void ClearChoices()
    {
        foreach (EventChoiceUI choiceUI in choiceUIs)
        {
            if (choiceUI != null)
                Destroy(choiceUI.gameObject);
        }

        choiceUIs.Clear();
    }
}
