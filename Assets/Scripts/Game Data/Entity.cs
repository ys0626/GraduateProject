using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 또는 적의 정보들을 저장하는 class
/// </summary>
[System.Serializable]
public class Entity : IBattleEntity
{
    // =====================================================
    // Entity가 저장하는 정보들
    // =====================================================
    
    // 최대 체력
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
                currentHP = maxHP;
            }
        }
    }


    // 현재 체력
    private int currentHP;

    public event Action<int> OnCurrentHPChanged;

    public event Action OnDead;

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
                OnDead?.Invoke();
            }
        }
    }


    // 최대 에너지
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


    // 현재 에너지
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


    // 현재 보유한 방어도
    private int block;

    public event Action<int> OnBlockChanged;

    public int Block
    {
        get => block;

        set
        {
            int clampedValue = Mathf.Max(0, value);

            if (block == clampedValue)
                return;

            block = clampedValue;

            OnBlockChanged?.Invoke(block);
        }
    }


    // 현재 보유한 버프, 디버프들
    public List<Status> statuses { get; private set; }
    = new List<Status>();

    public event Action OnStatusesChanged;




    // 덱, 손패, 뽑을 카드 더미, 버린 카드 더미, 소멸된 카드 더미
    public List<CardInstance> deck { get; private set; }
    = new List<CardInstance>();

    public List<CardInstance> hand { get; private set; }
    = new List<CardInstance>();

    public List<CardInstance> drawPile { get; private set; }
        = new List<CardInstance>();

    public List<CardInstance> discardPile { get; private set; }
        = new List<CardInstance>();

    public List<CardInstance> exhaustPile { get; private set; }
        = new List<CardInstance>();








    // =====================================================
    // 플레이어만 저장하는 정보들
    // =====================================================

    // 덱의 카드 수
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


    // 뽑을 카드 더미의 카드 수
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


    // 버린 카드 더미의 카드 수
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


    // 소멸된 카드 더미의 카드 수
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


    // 골드
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
    // 함수들
    // =====================================================


    /// <summary>
    /// amount만큼의 에너지를 사용한다.
    /// 에너지가 부족하면 false를 리턴.
    /// </summary>
    public bool TryUseEnergy(int amount)
    {
        if (CurrentEnergy < amount)
        {
            return false;
        }

        CurrentEnergy -= amount;

        return true;
    }


    /// <summary>
    /// Entity가 보유한 버프 또는 디버프의 value값을 받아옴
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public int GetStatusValue(StatusType type)
    {
        foreach (Status status in statuses)
        {
            if (status.Type == type)
            {
                return status.Value;
            }
        }

        return 0;
    }


    // <summary>
    /// 이 Entity가 버프 또는 디버프를 획득
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    public void AddStatus(Status newStatus)
    {
        // 같은 상태가 이미 있으면 합치기
        foreach (Status status in statuses)
        {
            if (status.Type == newStatus.Type)
            {
                status.AddValue(newStatus.Value);

                OnStatusesChanged?.Invoke();

                return;
            }
        }

        statuses.Add(newStatus);

        OnStatusesChanged?.Invoke();
    }


    /// <summary>
    /// 매 턴 종료 시 Entity가 보유한 디버프의 턴 수 -1
    /// </summary>
    public void TickStatuses()
    {
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            Status status = statuses[i];

            status.OnTurnEnd(this);

            if (status.ShouldRemove())
            {
                statuses.RemoveAt(i);
            }
        }

        OnStatusesChanged?.Invoke();
    }


    /// <summary>
    /// 버프와 디버프를 고려하여 이 Entity가 입힐 damage를 계산
    /// </summary>
    /// <param name="baseDamage"></param>
    /// <returns></returns>
    public int CalculateDamage(int baseDamage)
    {
        int damage = baseDamage;
        
        // 힘 적용
        damage += GetStatusValue(StatusType.Strength);

        // 약화 적용
        if (GetStatusValue(StatusType.Weak) > 0)
        {
            damage = Mathf.FloorToInt(damage * 0.75f);
        }

        return Mathf.Max(0, damage);
    }


    /// <summary>
    /// 버프와 디버프를 고려하여 이 Entity가 얻을 block을 계산
    /// </summary>
    /// <param name="baseBlock"></param>
    /// <returns></returns>
    public int CalculateBlock(int baseBlock)
    {
        int block = baseBlock;

        // 민첩 적용
        block += GetStatusValue(StatusType.Dexterity);

        return Mathf.Max(0, block);
    }


    /// <summary>
    /// 버프와 디버프를 고려하여 이 Entity가 피해를 입음
    /// </summary>
    /// <param name="baseDamage"></param>
    public void TakeDamage(int damage)
    {
        // 취약 적용
        if (GetStatusValue(StatusType.Vulnerable) > 0)
        {
            damage =
                Mathf.FloorToInt(damage * 1.5f);
        }

        ApplyDamage(damage);
    }


    /// <summary>
    /// Entity가 방어도를 얻음
    /// </summary>
    /// <param name="block"></param>
    public void GetBlock(int block)
    {
        ApplyBlock(block);
    }


    /// <summary>
    /// 실제로 damage만큼의 피해를 입음
    /// 방어도를 보유하고 있다면 방어도가 피해를 먼저 흡수함
    /// </summary>
    /// <param name="damage"></param>
    private void ApplyDamage(int damage)
    {
        if (damage <= 0)
            return;

        int remaining = damage;

        // Block으로 흡수
        if (Block > 0)
        {
            int absorbed = Mathf.Min(Block, remaining);

            Block -= absorbed;
            
            remaining -= absorbed;
        }

        // 남은 피해만큼 HP 감소
        if (remaining > 0)
        {
            CurrentHP -= remaining;
        }
    }


    /// <summary>
    /// 실제로 block만큼의 방어도를 얻음
    /// </summary>
    /// <param name="block"></param>
    private void ApplyBlock(int block)
    {
        if (block <= 0)
            return;

        Block += block;
    }

}