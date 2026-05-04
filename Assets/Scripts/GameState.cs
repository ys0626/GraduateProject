using System.Collections.Generic;


public class GameState
{
    public Entity player;
    public Entity enemy;
    public List<Card> deck;
    public List<Card> hand;
    public List<Card> discardPile;
    public int currentEnergy;
    public int maxEnergy;
    public int turnCount;
    public bool isPlayerTurn;

    public bool IsTerminal()
    {
        return player.currentHP <= 0 || enemy.currentHP <= 0;
    }
}
