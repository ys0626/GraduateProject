
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

        // 게임 데이터 초기화
        if (GameData.instance != null)
        {
            GameData.instance.InitNewGame();
        }
        else
        {
            Debug.LogError(
                "[GameSessionManager] GameData.instance가 없습니다."
            );
        }

        // 기존 맵이 존재하면 새로운 맵 생성
        if (MapGenerator.instance != null)
        {
            MapGenerator.instance.RegenerateMap();
        }
        else
        {
            Debug.Log(
                "[GameSessionManager] MapGenerator가 아직 생성되지 않았습니다. " +
                "MapScene 진입 시 맵을 생성합니다."
            );
        }

        Debug.Log("[GameSessionManager] 새로운 게임을 시작합니다.");
    }


    // 현재 씬을 기록한다.
    public void RecordCurrentScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        // 활성 런이 없으면 씬 정보를 기록하지 않음
        if (!HasActiveRun)
            return;

        // 메인 메뉴는 이어하기 위치로 기록하지 않음
        if (sceneName == "MainMenu" || sceneName == "MainMenuScene")
            return;

        if (sceneName == "BattleScene")
        {
            resumeSceneName = sceneBeforeBattle;
        }
        else
        {
            resumeSceneName = sceneName;
            sceneBeforeBattle = sceneName;
        }
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

    public void EndRun()
    {
        HasActiveRun = false;

        resumeSceneName = "MapScene";
        sceneBeforeBattle = "MapScene";

        Debug.Log("현재 런이 종료되었습니다.");
    }
}