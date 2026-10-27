
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingPanelUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingRoot;
    [SerializeField] private Button popupBackgroundButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private TMP_Text exitButtonText;
    [SerializeField] private Color enabledTextColor = Color.white;
    [SerializeField]
    private Color disabledTextColor =
        new Color(0.45f, 0.45f, 0.45f, 0.6f);


    private void Awake()
    {
        if (popupBackgroundButton != null)
        {
            popupBackgroundButton.onClick.AddListener(Close);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(GoToMainMenu);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        UpdateExitButton();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateExitButton();
    }

    private void UpdateExitButton()
    {
        if (exitButton == null)
            return;

        bool canExitToMainMenu =
            SceneManager.GetActiveScene().name == "MapScene";

        exitButton.interactable = canExitToMainMenu;

        if (exitButtonText != null)
        {
            exitButtonText.color = canExitToMainMenu
                ? enabledTextColor
                : disabledTextColor;
        }
    }

    public void Toggle()
    {
        if (settingRoot == null)
        {
            Debug.LogError(
                "[SettingPanelUI] Setting Root가 연결되지 않았습니다."
            );
            return;
        }

        settingRoot.SetActive(!settingRoot.activeSelf);
    }

    public void Close()
    {
        if (settingRoot != null)
        {
            settingRoot.SetActive(false);
        }
    }

    private void GoToMainMenu()
    {
        // 혹시 비활성화된 버튼의 이벤트가 호출되는 상황도 방지한다.
        if (SceneManager.GetActiveScene().name != "MapScene")
            return;

        Close();

        if (SceneChanger.instance == null)
        {
            Debug.LogError(
                "[SettingPanelUI] SceneChanger가 없습니다."
            );
            return;
        }

        SceneChanger.instance.GoToMainMenuScene();
    }
}
