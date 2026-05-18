using System.Collections.Generic;

public interface IBattleEntity
{
    // =====================================================
    // 기본 스탯
    // =====================================================

    int MaxHP { get; set; }

    int CurrentHP { get; set; }

    int MaxEnergy { get; set; }

    int CurrentEnergy { get; set; }

    int Block { get; set; }

    // =====================================================
    // 상태 이상
    // =====================================================

    List<Status> statuses { get; }

    // =====================================================
    // 카드 더미
    // =====================================================

    List<CardInstance> hand { get; }

    List<CardInstance> drawPile { get; }

    List<CardInstance> discardPile { get; }

    List<CardInstance> exhaustPile { get; }

    // =====================================================
    // 기능
    // =====================================================

    bool TryUseEnergy(int amount);

    int GetStatusValue(StatusType type);

    void AddStatus(Status status);

    void TickStatuses();

    int CalculateDamage(int baseDamage);

    int CalculateBlock(int baseBlock);

    void TakeDamage(int damage);

    void GetBlock(int block);
}