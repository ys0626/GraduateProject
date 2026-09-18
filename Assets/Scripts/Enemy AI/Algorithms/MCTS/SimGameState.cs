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

    // 캐시 재사용 여부를 결정하는 전투 상태 해시.
    // 롤아웃 결과와 다음 행동에 영향을 주는 값은 모두 포함한다.
    public ulong GetStateHash(TurnPlayHistory turnHistory)
    {
        unchecked
        {
            ulong hash = 17;

            hash = HashEntity(hash, self);
            hash = HashEntity(hash, opponent);

            hash = hash * 31 + (ulong)(selfTurn ? 1 : 0);
            hash = hash * 31 + (ulong)turnCount;
            hash = HashTurnHistory(hash, turnHistory);

            // 손패 순서는 행동에 영향을 주지 않지만, 드로우 더미 순서는 다음 드로우에 영향을 준다.
            hash = HashCards(hash, self.hand, orderMatters: false);
            hash = HashCards(hash, opponent.hand, orderMatters: false);
            hash = HashCards(hash, self.drawPile, orderMatters: true);
            hash = HashCards(hash, opponent.drawPile, orderMatters: true);
            hash = HashCards(hash, self.discardPile, orderMatters: true);
            hash = HashCards(hash, opponent.discardPile, orderMatters: true);
            hash = HashCards(hash, self.exhaustPile, orderMatters: true);
            hash = HashCards(hash, opponent.exhaustPile, orderMatters: true);

            return hash;
        }
    }

    private static ulong HashEntity(ulong hash, SimEntity entity)
    {
        unchecked
        {
            hash = hash * 31 + (ulong)entity.MaxHP;
            hash = hash * 31 + (ulong)entity.CurrentHP;
            hash = hash * 31 + (ulong)entity.MaxEnergy;
            hash = hash * 31 + (ulong)entity.CurrentEnergy;
            hash = hash * 31 + (ulong)entity.Block;
            hash = hash * 31 + (ulong)entity.DoubleTapCharges;
            hash = HashStatuses(hash, entity);
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

    private static ulong HashTurnHistory(ulong hash, TurnPlayHistory history)
    {
        hash = hash * 31 + (ulong)(history.hasPlainAttack ? 1 : 0);
        hash = hash * 31 + (ulong)(history.hasAnyAttack ? 1 : 0);
        hash = hash * 31 + (ulong)(history.hasLimitBreak ? 1 : 0);
        hash = hash * 31 + (ulong)(history.hasDoubleTap ? 1 : 0);
        return hash;
    }

    private static ulong HashCards(
        ulong hash,
        List<CardInstance> cards,
        bool orderMatters)
    {
        List<CardInstance> cardsToHash = cards;

        if (!orderMatters)
        {
            cardsToHash = new List<CardInstance>(cards);
            cardsToHash.Sort((a, b) =>
            {
                int idA = a.data.GetHashCode();
                int idB = b.data.GetHashCode();
                return idA != idB ? idA.CompareTo(idB) : a.currentCost.CompareTo(b.currentCost);
            });
        }

        hash = hash * 31 + (ulong)cardsToHash.Count;

        foreach (CardInstance card in cardsToHash)
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
