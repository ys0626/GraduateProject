using System.Collections.Generic;

public class GameState
{
    public Entity player;
    public Entity enemy;

    public List<CardInstance> deck;
    public List<CardInstance> hand;
    public List<CardInstance> discardPile;
    public List<CardInstance> exhaustPile;

    public int currentEnergy;
    public int maxEnergy;
    public int turnCount;
    public bool isPlayerTurn;

    // 플레이어나 적 둘 중 하나 죽으면 true 반환
    public bool IsTerminal()
    {
        return player.currentHP <= 0 || enemy.currentHP <= 0;
    }

    // MCTS용 게임 상태 복사
    public GameState Clone()
    {
        return new GameState
        {
            player = player.Clone(),
            enemy = enemy.Clone(),
            deck = new List<CardInstance>(deck),
            hand = new List<CardInstance>(hand),
            discardPile = new List<CardInstance>(discardPile),
            exhaustPile = new List<CardInstance>(exhaustPile),
            currentEnergy = this.currentEnergy,
            maxEnergy = this.maxEnergy,
            turnCount = this.turnCount,
            isPlayerTurn = this.isPlayerTurn
        };
    }
}
