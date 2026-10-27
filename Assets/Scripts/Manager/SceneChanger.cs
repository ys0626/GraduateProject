
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger instance;

    [Header("Fade")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;

    public bool IsReady { get; private set; } = false;

    private bool isChangingScene = false;
    private bool battleScenePrepared = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (fadeImage != null)
        {
            SetAlpha(1f);
            fadeImage.gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        IsReady = true;

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
        }
    }

    public void GoToMapScene()
    {
        ChangeScene("MapScene");
    }

    public void GoToBattleScene()
    {
        ChangeScene("BattleScene");
    }

    public void GoToShopScene()
    {
        ChangeScene("ShopScene");
    }

    public void GoToEventScene()
    {
        ChangeScene("EventScene");
    }

    public void GoToRestScene()
    {
        ChangeScene("RestScene");
    }
    public void GoToMainMenuScene()
    {
        ChangeScene("MainMenuScene");
    }

    public void ResumeGame()
    {
        if (GameSessionManager.instance == null)
        {
            Debug.LogError(
                "[SceneChanger] GameSessionManager가 없습니다."
            );

            return;
        }

        string sceneName =
            GameSessionManager.instance.GetResumeSceneName();

        ChangeScene(sceneName);
    }

    public void ChangeScene(string sceneName)
    {
        if (isChangingScene)
            return;

        if (GameSessionManager.instance != null)
        {
            GameSessionManager.instance.RecordCurrentScene(
                SceneManager.GetActiveScene().name
            );
        }

        StartCoroutine(ChangeSceneRoutine(sceneName));
    }

    private IEnumerator ChangeSceneRoutine(string sceneName)
    {
        isChangingScene = true;
        IsReady = false;
        battleScenePrepared = false;

        // 1. 현재 씬 페이드아웃
        yield return Fade(0f, 1f);

        // 2. 새 씬 로드
        AsyncOperation operation =
            SceneManager.LoadSceneAsync(sceneName);

        yield return new WaitUntil(() => operation.isDone);

        // 3. 새 씬의 오브젝트가 초기화될 때까지 대기
        yield return null;
        yield return new WaitForEndOfFrame();

        // 4. 배틀씬은 전투 준비가 완료될 때까지 검은 화면 유지
        if (sceneName == "BattleScene")
        {
            yield return new WaitUntil(() => battleScenePrepared);
        }

        // 5. 준비 완료 후 페이드인
        yield return Fade(1f, 0f);

        // 6. 화면 표시 완료
        IsReady = true;
        isChangingScene = false;
    }

    // GameManager가 전투 준비를 끝낸 후 호출
    public void NotifyBattleScenePrepared()
    {
        battleScenePrepared = true;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        if (fadeImage == null)
        {
            Debug.LogError(
                "SceneChanger: Fade Image is not assigned."
            );

            yield break;
        }

        fadeImage.gameObject.SetActive(true);
        fadeImage.raycastTarget = true;

        float elapsed = 0f;

        SetAlpha(startAlpha);

        yield return null;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / fadeDuration);

            SetAlpha(Mathf.Lerp(startAlpha, endAlpha, t));

            yield return null;
        }

        SetAlpha(endAlpha);

        if (endAlpha <= 0f)
        {
            fadeImage.raycastTarget = false;
            fadeImage.gameObject.SetActive(false);
        }
    }

    private void SetAlpha(float alpha)
    {
        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }







    //테스트용
    [ContextMenu("Debug/Force Move To ShopScene")]
    private void ForceMoveToShopScene()
    {
        isChangingScene = false;
        ChangeScene("ShopScene");
    }
}
