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
        //TODO: 폴리노미얼 롤링 해시 구현(각 상태를 하나의 숫자로)
        return 0;
    }

    private static ulong HashStatuses(ulong hash, SimEntity entity)
    {
        //TODO: 
        return hash;
    }

    private static ulong HashHand(ulong hash, List<CardInstance> hand)
    {
        //TODO: 손에 있는 패를 정렬
        return hash;
    }
}