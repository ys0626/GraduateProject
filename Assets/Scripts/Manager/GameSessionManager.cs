
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager instance;

    // 게임을 시작했거나 진행 중인 상태인지
    public bool HasActiveRun { get; private set; }

    // 이어하기 시 이동할 씬
    private string resumeSceneName = "MapScene";

    // BattleScene에 진입하기 직전의 씬
    private string sceneBeforeBattle = "MapScene";

    // 새 게임 시작
    public void StartNewGame()
    {
        HasActiveRun = true;
        resumeSceneName = "MapScene";
        sceneBeforeBattle = "MapScene";

        if (GameData.instance != null)
        {
            GameData.instance.InitNewGame();
        }
    }

    // 현재 씬을 기록한다.

    public void RecordCurrentScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        // 메인 메뉴는 게임 진행 씬이 아니므로
        // 기존 복귀 씬 정보를 덮어쓰지 않는다.
        if (sceneName == "MainMenu")
            return;

        // 전투 중 메인 메뉴로 나가는 경우
        // 전투 직전 씬으로 복귀하도록 기록한다.
        if (sceneName == "BattleScene")
        {
            resumeSceneName = sceneBeforeBattle;
            return;
        }

        // 일반적인 게임 씬은 그대로 복귀 대상으로 기록한다.
        resumeSceneName = sceneName;

        // 다음 전투를 위한 직전 씬 기록
        sceneBeforeBattle = sceneName;
    }


    // 메인 메뉴에서 게임 계속하기를 눌렀을 때
    public string GetResumeSceneName()
    {
        if (string.IsNullOrEmpty(resumeSceneName) ||
            resumeSceneName == "MainMenu" ||
            resumeSceneName == "BattleScene")
        {
            return "MapScene";
        }

        return resumeSceneName;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartNewGameAndGoToMap()
    {
        StartNewGame();

        if (SceneChanger.instance != null)
        {
            SceneChanger.instance.GoToMapScene();
        }
        else
        {
            Debug.LogError(
                "[GameSessionManager] SceneChanger가 없습니다."
            );
        }
    }
}
