using System;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Persistent Objects")]
    public GameObject[] persistentObjects;

    private void Awake()
    {
        if(instance != null)
        {
            CleanUpAndDestroy();
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            MarkPersistentObjects();
        }
    }

    private void Start()
    {
        //테스트용 세팅값 적용
        InitGameData();

        // 자동 시뮬레이션 아닐 때만 초기 UI 갱신
        if (!SimulationManager.instance.AutoSimulation)
        {
            UIManager.instance.UpdateAll();
        }

        //배틀 시작
        BattleManager.instance.Init();
    }


    /// <summary>
    /// 테스트용 세팅값 적용
    /// </summary>
    private void InitGameData()
    {
        SimulationManager sim = SimulationManager.instance;

        // =================================================
        // 플레이어 덱
        // =================================================

        foreach (CardData cardData in sim.PlayerStarterDeck)
        {
            GameData.instance.player.deck
                .Add(new CardInstance(cardData));
        }

        PlayerDeckManager.instance.InitPlayerDeck();

        // =================================================
        // 적 덱
        // =================================================

        foreach (CardData cardData in sim.EnemyStarterDeck)
        {
            GameData.instance.enemy.deck
                .Add(new CardInstance(cardData));
        }

        EnemyDeckManager.instance.InitEnemyDeck();

        // =================================================
        // 스탯
        // =================================================

        GameData.instance.player.MaxHP =
            sim.PlayerMaxHP;

        GameData.instance.player.MaxEnergy =
            sim.PlayerMaxEnergy;

        GameData.instance.enemy.MaxHP =
            sim.EnemyMaxHP;

        GameData.instance.enemy.MaxEnergy =
            sim.EnemyMaxEnergy;

        // =================================================
        // 기타 초기화
        // =================================================

        GameData.instance.Init();
    }

    /// <summary>
    /// 씬 전환 시에 유지할 게임 오브젝트들 처리
    /// </summary>
    void MarkPersistentObjects()
    {
        foreach (GameObject obj in persistentObjects)
        {
            if(obj != null)
            {
                DontDestroyOnLoad(obj);
            }
        }
    }

    /// <summary>
    ///  씬 전환 시에 게임 오브젝트들 중복 생성 방지
    /// </summary>
    void CleanUpAndDestroy()
    {
        foreach (GameObject obj in persistentObjects)
        {
            Destroy(obj);
        }
        Destroy(gameObject);
    }
}