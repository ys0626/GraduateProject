using System;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

/// <summary>
/// db 테스트용 class(db로 대체 필요)
/// </summary>
public class DBTest : MonoBehaviour
{
    public static DBTest instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else Destroy(gameObject);

        maxHP = 66;
        currentHP = maxHP;
        Gold = 0;
        DeckCount = playerDeck.Count;
        DrawPileCount = drawPile.Count;
        DiscardPileCount = discardPile.Count;
        ExhaustPileCount = exhaustPile.Count;
        MaxEnergy = 3;
        currentEnergy = 3;
        enemyMaxHP = 27;
        enemyCurrentHP = enemyMaxHP;
    }

    // =====================================================
    // 플레이어의 Max HP
    // =====================================================

    private int maxHP;

    public event Action<int> OnMaxHPChanged;

    public int MaxHP
    {
        get => maxHP;

        set
        {
            int clampedValue = Mathf.Max(0, value);

            if (maxHP == clampedValue)
                return;

            maxHP = clampedValue;

            OnMaxHPChanged?.Invoke(maxHP);

            // 현재 HP가 최대 HP보다 크면 보정
            if (currentHP > maxHP)
            {
                CurrentHP = maxHP;
            }
        }
    }

    // =====================================================
    // 플레이어의 Current HP
    // =====================================================

    private int currentHP;

    public event Action<int> OnCurrentHPChanged;

    public event Action OnPlayerDead;

    public int CurrentHP
    {
        get => currentHP;

        set
        {
            int clampedValue = Mathf.Clamp(value, 0, maxHP);

            if (currentHP == clampedValue)
                return;

            currentHP = clampedValue;

            OnCurrentHPChanged?.Invoke(currentHP);

            if (currentHP == 0)
            {
                OnPlayerDead?.Invoke();
            }
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
            int clampedValue = Mathf.Max(0, value);

            if (gold == clampedValue)
                return;

            gold = clampedValue;

            OnGoldChanged?.Invoke(gold);
        }
    }

    // =====================================================
    // Deck Count
    // =====================================================

    private int deckCount;

    public event Action<int> OnDeckCountChanged;

    public int DeckCount
    {
        get => deckCount;

        set
        {
            int clampedValue = Mathf.Max(0, value);

            if (deckCount == clampedValue)
                return;

            deckCount = clampedValue;

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
            int clampedValue = Mathf.Max(0, value);

            if (drawPileCount == clampedValue)
                return;

            drawPileCount = clampedValue;

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
            int clampedValue = Mathf.Max(0, value);

            if (discardPileCount == clampedValue)
                return;

            discardPileCount = clampedValue;

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
            int clampedValue = Mathf.Max(0, value);

            if (exhaustPileCount == clampedValue)
                return;

            exhaustPileCount = clampedValue;

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
            int clampedValue = Mathf.Max(0, value);

            if (maxEnergy == clampedValue)
                return;

            maxEnergy = clampedValue;

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
            int clampedValue = Mathf.Max(0, value);

            if (currentEnergy == clampedValue)
                return;

            currentEnergy = clampedValue;

            OnCurrentEnergyChanged?.Invoke(currentEnergy);
        }
    }


    // =====================================================
    // 플레이어의 덱, 손패, 드로우파일, 버린 카드 더미, 소멸된 카드
    // =====================================================
    public List<CardInstance> playerDeck = new List<CardInstance>();

    public List<CardInstance> hand = new List<CardInstance>();

    public List<CardInstance> drawPile = new List<CardInstance>();

    public List<CardInstance> discardPile = new List<CardInstance>();

    public List<CardInstance> exhaustPile = new List<CardInstance>();


    // =====================================================
    // 적의 Max HP
    // =====================================================

    private int enemyMaxHP;

    public event Action<int> OnEnemyMaxHPChanged;

    public int EnemyMaxHP
    {
        get => enemyMaxHP;

        set
        {
            int clampedValue = Mathf.Max(0, value);

            if (enemyMaxHP == clampedValue)
                return;

            enemyMaxHP = clampedValue;

            OnEnemyMaxHPChanged?.Invoke(enemyMaxHP);

            // 현재 HP가 최대 HP보다 크면 보정
            if (enemyCurrentHP > enemyMaxHP)
            {
                enemyCurrentHP = enemyMaxHP;
            }
        }
    }

    // =====================================================
    // 적의 Current HP
    // =====================================================

    private int enemyCurrentHP;

    public event Action<int> OnEnemyCurrentHPChanged;

    public event Action OnEnemyDead;

    public int EnemyCurrentHP
    {
        get => enemyCurrentHP;

        set
        {
            int clampedValue = Mathf.Clamp(value, 0, enemyMaxHP);

            if (enemyCurrentHP == clampedValue)
                return;

            enemyCurrentHP = clampedValue;

            OnEnemyCurrentHPChanged?.Invoke(enemyCurrentHP);

            if (enemyCurrentHP == 0)
            {
                OnEnemyDead?.Invoke();
            }
        }
    }
}
