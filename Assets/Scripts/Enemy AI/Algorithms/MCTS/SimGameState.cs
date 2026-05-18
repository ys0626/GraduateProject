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
}