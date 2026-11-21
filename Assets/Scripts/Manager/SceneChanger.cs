
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

        // 1. BGM 페이드 아웃 시작
        Coroutine bgmFadeCoroutine = null;

        if (BGMManager.instance != null)
        {
            bgmFadeCoroutine = BGMManager.instance.FadeOut();
        }

        // 2. 화면과 BGM을 동시에 페이드 아웃
        yield return Fade(0f, 1f);

        // BGM 페이드 아웃이 끝날 때까지 대기
        if (bgmFadeCoroutine != null)
        {
            yield return bgmFadeCoroutine;
        }

        // 3. 새 씬 로드
        AsyncOperation operation =
            SceneManager.LoadSceneAsync(sceneName);

        yield return new WaitUntil(() => operation.isDone);

        // 4. 새 씬의 오브젝트 초기화 대기
        yield return null;
        yield return new WaitForEndOfFrame();

        // 5. 배틀씬은 전투 준비가 완료될 때까지 대기
        if (sceneName == "BattleScene")
        {
            yield return new WaitUntil(() => battleScenePrepared);
        }

        // 6. 새 씬의 BGM 재생 시작
        if (BGMManager.instance != null)
        {
            BGMManager.instance.PlaySceneBGM(sceneName);
        }

        // 7. 화면 페이드 인
        yield return Fade(1f, 0f);

        // 8. 씬 전환 완료
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







    // =========================================================
    // Debug
    // =========================================================

    [ContextMenu("Debug/Force Move To ShopScene")]
    private void ForceMoveToShopScene()
    {
        isChangingScene = false;
        ChangeScene("ShopScene");
    }

    [ContextMenu("Debug/Force Move To Elite Battle")]
    private void ForceMoveToEliteBattle()
    {
        if (GameData.instance == null)
        {
            Debug.LogError(
                "[SceneChanger] GameData가 없습니다."
            );

            return;
        }

        GameData.instance.CurrentBattleType = BattleType.Elite;

        isChangingScene = false;
        ChangeScene("BattleScene");

        Debug.Log("[SceneChanger] Elite Battle Debug Started");
    }

    [ContextMenu("Debug/Force Move To Boss Battle")]
    private void ForceMoveToBossBattle()
    {
        if (GameData.instance == null)
        {
            Debug.LogError(
                "[SceneChanger] GameData가 없습니다."
            );

            return;
        }

        GameData.instance.CurrentBattleType = BattleType.Boss;

        isChangingScene = false;
        ChangeScene("BattleScene");

        Debug.Log("[SceneChanger] Boss Battle Debug Started");
    }

    [ContextMenu("Debug/Force Move To EventScene")]
    private void ForceMoveToEventScene()
    {
        isChangingScene = false;
        ChangeScene("EventScene");

        Debug.Log("[SceneChanger] Event Scene Debug Started");
    }

    [ContextMenu("Debug/Force Move To RestScene")]
    private void ForceMoveToRestScene()
    {
        isChangingScene = false;
        ChangeScene("RestScene");

        Debug.Log("[SceneChanger] Rest Scene Debug Started");
    }
}
