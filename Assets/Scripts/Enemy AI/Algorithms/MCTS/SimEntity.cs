using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MCTS / 시뮬레이션 전용 Entity
/// UI, 이벤트 없이 순수 게임 상태만 저장
/// </summary>
public class SimEntity : IBattleEntity
{
    // =====================================================
    // 기본 스탯
    // =====================================================

    public int MaxHP { get; set; }

    public int CurrentHP { get; set; }

    public int MaxEnergy { get; set; }

    public int CurrentEnergy { get; set; }

    public int Block { get; set; }

    // =====================================================
    // 상태 이상
    // =====================================================

    public List<Status> statuses { get; private set; }
    = new List<Status>();

    // =====================================================
    // 카드 더미
    // =====================================================

    public List<CardInstance> hand { get; private set; }
    = new List<CardInstance>();

    public List<CardInstance> drawPile { get; private set; }
        = new List<CardInstance>();

    public List<CardInstance> discardPile { get; private set; }
        = new List<CardInstance>();

    public List<CardInstance> exhaustPile { get; private set; }
        = new List<CardInstance>();

    // =====================================================
    // 정보를 그대로 복사한 SimEntity 생성
    // =====================================================

    public static SimEntity Clone(IBattleEntity iBattleEntity)
    {
        SimEntity clone = new SimEntity();

        // 기본 스탯 복사
        clone.MaxHP = iBattleEntity.MaxHP;
        clone.CurrentHP = iBattleEntity.CurrentHP;

        clone.MaxEnergy = iBattleEntity.MaxEnergy;
        clone.CurrentEnergy = iBattleEntity.CurrentEnergy;

        clone.Block = iBattleEntity.Block;

        // =====================================================
        // 상태 이상 복사
        // =====================================================

        foreach (Status status in iBattleEntity.statuses)
        {
            clone.statuses.Add(status.Clone());
        }

        // =====================================================
        // 카드 더미 복사
        // =====================================================

        foreach (CardInstance card in iBattleEntity.hand)
        {
            clone.hand.Add(card.Clone());
        }

        foreach (CardInstance card in iBattleEntity.drawPile)
        {
            clone.drawPile.Add(card.Clone());
        }

        foreach (CardInstance card in iBattleEntity.discardPile)
        {
            clone.discardPile.Add(card.Clone());
        }

        foreach (CardInstance card in iBattleEntity.exhaustPile)
        {
            clone.exhaustPile.Add(card.Clone());
        }

        return clone;
    }



    // =====================================================
    // 에너지 사용
    // =====================================================

    public bool TryUseEnergy(int amount)
    {
        if (CurrentEnergy < amount)
        {
            return false;
        }

        CurrentEnergy -= amount;

        return true;
    }

    // =====================================================
    // 상태 이상 관련
    // =====================================================

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

    public void AddStatus(Status newStatus)
    {
        foreach (Status status in statuses)
        {
            if (status.Type == newStatus.Type)
            {
                status.AddValue(newStatus.Value);

                return;
            }
        }

        statuses.Add(newStatus);
    }

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
    }

    // =====================================================
    // 데미지 계산
    // =====================================================

    public int CalculateDamage(int baseDamage)
    {
        int damage = baseDamage;

        // 힘 적용
        damage += GetStatusValue(StatusType.Strength);

        // 약화 적용
        if (GetStatusValue(StatusType.Weak) > 0)
        {
            damage =
                Mathf.FloorToInt(damage * 0.75f);
        }

        return Mathf.Max(0, damage);
    }

    // =====================================================
    // 방어도 계산
    // =====================================================

    public int CalculateBlock(int baseBlock)
    {
        int block = baseBlock;

        // 민첩 적용
        block += GetStatusValue(StatusType.Dexterity);

        return Mathf.Max(0, block);
    }

    // =====================================================
    // 피해 / 방어도 적용
    // =====================================================

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

    public void GetBlock(int block)
    {
        ApplyBlock(block);
    }

    private void ApplyDamage(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        int remaining = damage;

        // Block으로 흡수
        if (Block > 0)
        {
            int absorbed =
                Mathf.Min(Block, remaining);

            Block -= absorbed;

            remaining -= absorbed;
        }

        // 남은 피해만큼 HP 감소
        if (remaining > 0)
        {
            CurrentHP -= remaining;
        }
    }

    private void ApplyBlock(int block)
    {
        if (block <= 0)
        {
            return;
        }

        Block += block;
    }




    // =====================================================
    // 카드 더미들 관리
    // =====================================================

    // =====================================================
    // 카드 드로우
    // =====================================================

    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            DrawCard();
        }
    }

    public void DrawCard()
    {
        // 1. 손패 최대 제한 (10장)
        if (hand.Count >= 10)
        {
            return;
        }

        // 2. 드로우 더미가 비어있으면 재구성 시도
        if (drawPile.Count == 0)
        {
            // discard도 없으면 드로우 불가
            if (discardPile.Count == 0)
            {
                return;
            }

            ReshuffleDiscardPile();
        }

        // 3. 재구성 후에도 없으면 종료 (안전장치)
        if (drawPile.Count == 0)
            return;

        // 4. 손패 재확인 (리셋 상황 대비)
        if (hand.Count >= 10)
            return;

        CardInstance card = drawPile[0];

        drawPile.Remove(card);
        hand.Add(card);
    }

    // =====================================================
    // 카드 버리기
    // =====================================================

    public void DiscardCard(CardInstance card)
    {
        // 손패에서 제거
        hand.Remove(card);

        // 데이터 이동
        discardPile.Add(card);
    }

    // =====================================================
    // 카드 소멸
    // =====================================================

    public void ExhaustCard(CardInstance card)
    {
        // 손패에서 제거
        hand.Remove(card);

        // 데이터 이동
        exhaustPile.Add(card);
    }


    // =====================================================
    // 뽑을 카드 더미 섞기
    // =====================================================

    public void ShuffleDrawPile()
    {
        for (int i = 0; i < drawPile.Count; i++)
        {
            int randomIndex =
                Random.Range(i, drawPile.Count);

            CardInstance temp = drawPile[i];
            drawPile[i] = drawPile[randomIndex];
            drawPile[randomIndex] = temp;
        }
    }

    // =====================================================
    // 뽑을 카드 더미에서 카드가 부족한 경우,
    //  버린 카드 더미의 카드들을 뽑을 카드 더미로 보내고 뽑을 카드 더미를 섞기
    // =====================================================

    private void ReshuffleDiscardPile()
    {
        foreach (CardInstance card in discardPile)
        {
            drawPile.Add(card);
        }

        discardPile.Clear();

        ShuffleDrawPile();
    }

    public void DiscardHand()
    {
        while (hand.Count > 0)
        {
            CardInstance card =
                hand[
                    hand.Count - 1];

            DiscardCard(card);
        }
    }
}