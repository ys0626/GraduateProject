
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button continueGameButton;
    [SerializeField] private Button exitGameButton;

    private void Start()
    {
        RefreshButtons();
    }

    public void RefreshButtons()
    {
        bool hasActiveRun =
            GameSessionManager.instance != null &&
            GameSessionManager.instance.HasActiveRun;

        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(!hasActiveRun);
        }

        if (continueGameButton != null)
        {
            continueGameButton.gameObject.SetActive(hasActiveRun);
        }
    }

    public void OnStartGameClicked()
    {
        if (GameSessionManager.instance == null)
        {
            Debug.LogError(
                "[MainMenuUI] GameSessionManager가 없습니다."
            );
            return;
        }

        GameSessionManager.instance.StartNewGameAndGoToMap();
    }

    public void OnContinueGameClicked()
    {
        if (GameSessionManager.instance == null)
        {
            Debug.LogError(
                "[MainMenuUI] GameSessionManager가 없습니다."
            );
            return;
        }

        if (SceneChanger.instance == null)
        {
            Debug.LogError(
                "[MainMenuUI] SceneChanger가 없습니다."
            );
            return;
        }

        SceneChanger.instance.ResumeGame();
    }

    public void OnExitGameClicked()
    {
        Debug.Log("[MainMenuUI] 게임을 종료합니다.");

        Application.Quit();
    }
}
