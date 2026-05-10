using System;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Test Settings")]
    [Header("Player Setting")]
    [SerializeField] public CardData[] playerStarterDeck;
    [SerializeField] public int playerMaxHP;
    [SerializeField] public int playerMaxEnergy;

    [Header("Enemy Setting")]
    [SerializeField] public CardData[] enemyStarterDeck;
    [SerializeField] public int enemyMaxHP;
    [SerializeField] public int enemyMaxEnergy;

    [Header("Controller Setting")]
    [SerializeField] private ControllerType playerControllerType;
    [SerializeField] private ControllerType enemyControllerType;

    public ControllerType PlayerControllerType => playerControllerType;
    public ControllerType EnemyControllerType => enemyControllerType;


    public static GameManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        //테스트용 세팅값 적용
        InitGameData();

        //초기 UI 갱신
        UIManager.instance.UpdateAll();

        //배틀 시작
        BattleManager.instance.Init();
    }


    /// <summary>
    /// 테스트용 세팅값 적용
    /// </summary>
    private void InitGameData()
    {
        // 플레이어의 덱 세팅
        // (플레이어의 덱에 카드들을 추가하고, 덱의 카드들을 드로우 파일에 추가하고 섞기)
        foreach(CardData cardData in playerStarterDeck)
        {
            GameData.instance.player.deck.Add(new CardInstance(cardData));
        }
        PlayerDeckManager.instance.InitPlayerDeck();
        

        // 적의 덱 세팅
        // (적의 덱에 카드들을 추가하고, 덱의 카드들을 드로우 파일에 추가하고 섞기)
        foreach (CardData cardData in enemyStarterDeck)
        {
            GameData.instance.enemy.deck.Add(new CardInstance(cardData));
        }

        EnemyDeckManager.instance.InitEnemyDeck();


        //세팅값들 적용
        GameData.instance.player.MaxHP = playerMaxHP;
        GameData.instance.player.MaxEnergy = playerMaxEnergy;
        GameData.instance.enemy.MaxHP = enemyMaxHP;
        GameData.instance.enemy.MaxEnergy = enemyMaxEnergy;

        //그 외의 값들 초기화
        GameData.instance.Init();
    }
}