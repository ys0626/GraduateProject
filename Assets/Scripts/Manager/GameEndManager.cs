
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameEndManager : MonoBehaviour
{
    public static GameEndManager instance;

    [Header("Game End UI")]
    [SerializeField] private Image blackScreen;
    [SerializeField] private GameObject gameEndPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button mainMenuButton;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.0f;

    private bool isGameEnded;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        // 초기 상태 설정
        SetBlackScreenAlpha(0f);

        gameEndPanel.SetActive(false);

        // 메인 메뉴 버튼 이벤트 등록
        mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClicked);
        }
    }

    public void ShowDefeat()
    {
        ShowGameEnd("패배..");
    }

    public void ShowVictory()
    {
        ShowGameEnd("승리!");
    }

    private void ShowGameEnd(string message)
    {
        if (isGameEnded)
            return;

        isGameEnded = true;

        // Game End UI를 가장 앞쪽에 표시
        transform.SetAsLastSibling();

        titleText.text = message;

        gameEndPanel.SetActive(false);
        blackScreen.gameObject.SetActive(true);
        blackScreen.raycastTarget = true;

        StartCoroutine(GameEndRoutine());
    }

    private IEnumerator GameEndRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            SetBlackScreenAlpha(alpha);

            yield return null;
        }

        SetBlackScreenAlpha(1f);

        gameEndPanel.SetActive(true);
    }

    private void SetBlackScreenAlpha(float alpha)
    {
        Color color = blackScreen.color;
        color.a = alpha;
        blackScreen.color = color;
    }

    private void OnMainMenuButtonClicked()
    {
        if (GameSessionManager.instance != null)
        {
            GameSessionManager.instance.EndRun();
        }
        else
        {
            Debug.LogError(
                "GameSessionManager.instance가 없습니다."
            );
        }

        if (SceneChanger.instance != null)
        {
            SceneChanger.instance.GoToMainMenuScene();
        }
    }
}