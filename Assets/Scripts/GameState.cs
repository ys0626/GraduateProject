using System.Collections.Generic;

public class GameState
{
    public Entity player;
    public Entity enemy;
    public List<CardDatabase.CardEntry> deck;
    public List<CardDatabase.CardEntry> hand;
    public List<CardDatabase.CardEntry> discardPile;
    public int currentEnergy;
    public int maxEnergy;
    public int turnCount;
    public bool isPlayerTurn;


    //플레이어나 적 둘 중 하나 죽으면 1 반환하는 함수
    public bool IsTerminal()
    {
        return player.currentHP <= 0 || enemy.currentHP <= 0;
    }
}
