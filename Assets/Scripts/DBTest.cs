using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DB 테스트용 클래스 (DB로 대체 필요)
/// </summary>
public class DBTest : MonoBehaviour
{
    public static DBTest instance;

    public Entity player;
    public Entity enemy;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // 프로퍼티로 통일 (이벤트 정상 발생)
        MaxHP = 66;
        CurrentHP = MaxHP;
        Gold = 0;
        MaxEnergy = 3;
        CurrentEnergy = 3;
        EnemyMaxHP = 27;
        EnemyCurrentHP = EnemyMaxHP;

        // Entity 초기화
        player = new Entity
        {
            maxHP = MaxHP,
            currentHP = CurrentHP,
            block = 0,
            statusEffects = new Dictionary<EffectType, int>()
        };

        enemy = new Entity
        {
            maxHP = EnemyMaxHP,
            currentHP = EnemyCurrentHP,
            block = 0,
            statusEffects = new Dictionary<EffectType, int>()
        };

        // Count는 리스트 초기화 후 설정
        DeckCount = playerDeck.Count;
        DrawPileCount = drawPile.Count;
        DiscardPileCount = discardPile.Count;
        ExhaustPileCount = exhaustPile.Count;
    }

    // =====================================================
    // GameState → DBTest 동기화 (BattleManager.UpdateUI에서 호출)
    // =====================================================
    public void SyncFromGameState(GameState state)
    {
        // Entity 참조 동기화
        player = state.player;
        enemy = state.enemy;

        // HP 동기화
        CurrentHP = state.player.currentHP;
        EnemyCurrentHP = state.enemy.currentHP;

        // 에너지 동기화
        CurrentEnergy = state.currentEnergy;

        // 카드 더미 동기화
        drawPile = state.deck;
        hand = state.hand;
        discardPile = state.discardPile;
        exhaustPile = state.exhaustPile;

        // Count 업데이트
        DrawPileCount = drawPile.Count;
        DiscardPileCount = discardPile.Count;
        ExhaustPileCount = exhaustPile.Count;
    }

    // =====================================================
    // 플레이어 Max HP
    // =====================================================
    private int maxHP;

    public event Action<int> OnMaxHPChanged;

    public int MaxHP
    {
        get => maxHP;
        set
        {
            int clamped = Mathf.Max(0, value);
            if (maxHP == clamped) return;

            maxHP = clamped;
            OnMaxHPChanged?.Invoke(maxHP);

            if (currentHP > maxHP)
                CurrentHP = maxHP;
        }
    }

    // =====================================================
    // 플레이어 Current HP
    // =====================================================
    private int currentHP;

    public event Action<int> OnCurrentHPChanged;
    public event Action OnPlayerDead;

    public int CurrentHP
    {
        get => currentHP;
        set
        {
            int clamped = Mathf.Clamp(value, 0, maxHP);
            if (currentHP == clamped) return;

            currentHP = clamped;
            OnCurrentHPChanged?.Invoke(currentHP);

            if (currentHP == 0)
                OnPlayerDead?.Invoke();
        }
    }

    // =====================================================
    // Gold
    // =====================================================
    private int gold;

    public event Action<int> OnGoldChanged;

    public int Gold
    {
        get => gold;
        set
        {
            int clamped = Mathf.Max(0, value);
            if (gold == clamped) return;

            gold = clamped;
            OnGoldChanged?.Invoke(gold);
        }
    }

    // =====================================================
    // Deck Count (전체 덱)
    // =====================================================
    private int deckCount;

    public event Action<int> OnDeckCountChanged;

    public int DeckCount
    {
        get => deckCount;
        set
        {
            int clamped = Mathf.Max(0, value);
            if (deckCount == clamped) return;

            deckCount = clamped;
            OnDeckCountChanged?.Invoke(deckCount);
        }
    }

    // =====================================================
    // Draw Pile Count
    // =====================================================
    private int drawPileCount;

    public event Action<int> OnDrawPileCountChanged;

    public int DrawPileCount
    {
        get => drawPileCount;
        set
        {
            int clamped = Mathf.Max(0, value);
            if (drawPileCount == clamped) return;

            drawPileCount = clamped;
            OnDrawPileCountChanged?.Invoke(drawPileCount);
        }
    }

    // =====================================================
    // Discard Pile Count
    // =====================================================
    private int discardPileCount;

    public event Action<int> OnDiscardPileCountChanged;

    public int DiscardPileCount
    {
        get => discardPileCount;
        set
        {
            int clamped = Mathf.Max(0, value);
            if (discardPileCount == clamped) return;

            discardPileCount = clamped;
            OnDiscardPileCountChanged?.Invoke(discardPileCount);
        }
    }

    // =====================================================
    // Exhaust Pile Count
    // =====================================================
    private int exhaustPileCount;

    public event Action<int> OnExhaustPileCountChanged;

    public int ExhaustPileCount
    {
        get => exhaustPileCount;
        set
        {
            int clamped = Mathf.Max(0, value);
            if (exhaustPileCount == clamped) return;

            exhaustPileCount = clamped;
            OnExhaustPileCountChanged?.Invoke(exhaustPileCount);
        }
    }

    // =====================================================
    // Max Energy
    // =====================================================
    private int maxEnergy;

    public event Action<int> OnMaxEnergyChanged;

    public int MaxEnergy
    {
        get => maxEnergy;
        set
        {
            int clamped = Mathf.Max(0, value);
            if (maxEnergy == clamped) return;

            maxEnergy = clamped;
            OnMaxEnergyChanged?.Invoke(maxEnergy);
        }
    }

    // =====================================================
    // Current Energy
    // =====================================================
    private int currentEnergy;

    public event Action<int> OnCurrentEnergyChanged;

    public int CurrentEnergy
    {
        get => currentEnergy;
        set
        {
            int clamped = Mathf.Max(0, value);
            if (currentEnergy == clamped) return;

            currentEnergy = clamped;
            OnCurrentEnergyChanged?.Invoke(currentEnergy);
        }
    }

    // =====================================================
    // 카드 더미 리스트
    // =====================================================
    public List<CardInstance> playerDeck = new List<CardInstance>(); // 전체 덱
    public List<CardInstance> hand = new List<CardInstance>(); // 손패
    public List<CardInstance> drawPile = new List<CardInstance>(); // 드로우 파일
    public List<CardInstance> discardPile = new List<CardInstance>(); // 버린 카드
    public List<CardInstance> exhaustPile = new List<CardInstance>(); // 소멸 카드

    // =====================================================
    // 적 Max HP
    // =====================================================
    private int enemyMaxHP;

    public event Action<int> OnEnemyMaxHPChanged;

    public int EnemyMaxHP
    {
        get => enemyMaxHP;
        set
        {
            int clamped = Mathf.Max(0, value);
            if (enemyMaxHP == clamped) return;

            enemyMaxHP = clamped;
            OnEnemyMaxHPChanged?.Invoke(enemyMaxHP);

            if (enemyCurrentHP > enemyMaxHP)
                EnemyCurrentHP = enemyMaxHP;
        }
    }

    // =====================================================
    // 적 Current HP
    // =====================================================
    private int enemyCurrentHP;

    public event Action<int> OnEnemyCurrentHPChanged;
    public event Action OnEnemyDead;

    public int EnemyCurrentHP
    {
        get => enemyCurrentHP;
        set
        {
            int clamped = Mathf.Clamp(value, 0, enemyMaxHP);
            if (enemyCurrentHP == clamped) return;

            enemyCurrentHP = clamped;
            OnEnemyCurrentHPChanged?.Invoke(enemyCurrentHP);

            if (enemyCurrentHP == 0)
                OnEnemyDead?.Invoke();
        }
    }
}