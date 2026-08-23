using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// 시뮬레이션에 필요한 정보들 저장
/// </summary>
public class SimGameState
{
    public SimEntity self;
    public SimEntity opponent;

    public bool selfTurn;
    public int turnCount;

    public static SimGameState Create(Entity selfEntity)
    {
        Entity opponentEntity =
            GetOpponent(selfEntity);

        return new SimGameState
        {
            self = SimEntity.Clone(selfEntity),
            opponent = SimEntity.Clone(opponentEntity),
            selfTurn = true,
            turnCount = BattleManager.instance.turnCount
        };
    }

    public SimGameState Clone()
    {
        return new SimGameState
        {
            self = SimEntity.Clone(self),
            opponent = SimEntity.Clone(opponent),
            selfTurn = selfTurn,
            turnCount = turnCount
        };
    }


    private static Entity GetOpponent(Entity self)
    {
        return self == GameData.instance.player
            ? GameData.instance.enemy
            : GameData.instance.player;
    }

    // 동일 상태 판별용 해시.
    public ulong GetStateHash()
    {
        unchecked
        {
            ulong hash = 17;

            hash = hash * 31 + (ulong)self.CurrentHP;
            hash = hash * 31 + (ulong)self.CurrentEnergy;
            hash = hash * 31 + (ulong)self.Block;

            hash = hash * 31 + (ulong)opponent.CurrentHP;
            hash = hash * 31 + (ulong)opponent.CurrentEnergy;
            hash = hash * 31 + (ulong)opponent.Block;

            hash = hash * 31 + (ulong)(selfTurn ? 1 : 0);
            hash = hash * 31 + (ulong)turnCount;

            hash = HashStatuses(hash, self);
            hash = HashStatuses(hash, opponent);

            hash = HashHand(hash, self.hand);
            hash = HashHand(hash, opponent.hand);

            return hash;
        }
    }

    private static ulong HashStatuses(ulong hash, SimEntity entity)
    {
        hash = hash * 31 + (ulong)entity.GetStatusValue(StatusType.Strength);
        hash = hash * 31 + (ulong)entity.GetStatusValue(StatusType.Dexterity);
        hash = hash * 31 + (ulong)entity.GetStatusValue(StatusType.Weak);
        hash = hash * 31 + (ulong)entity.GetStatusValue(StatusType.Vulnerable);
        return hash;
    }

    private static ulong HashHand(ulong hash, List<CardInstance> hand)
    {
        var sorted = new List<CardInstance>(hand);
        sorted.Sort((a, b) =>
        {
            int idA = a.data.GetHashCode();
            int idB = b.data.GetHashCode();
            return idA != idB ? idA.CompareTo(idB) : a.currentCost.CompareTo(b.currentCost);
        });

        foreach (var card in sorted)
        {
            hash = hash * 31 + (ulong)card.data.GetHashCode();
            hash = hash * 31 + (ulong)card.currentCost;
            hash = hash * 31 + (ulong)(card.upgraded ? 1 : 0);
            hash = hash * 31 + (ulong)(card.exhaust ? 1 : 0);
            hash = hash * 31 + (ulong)(card.ethereal ? 1 : 0);
        }

        return hash;
    }
}