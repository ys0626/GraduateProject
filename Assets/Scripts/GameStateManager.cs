using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public void ShuffleDeck(List<CardDatabase.CardEntry> deck) { }

    public GameState DrawCards(GameState state, int count)
    {
        return state;  // 나중에 구현
    }

    public GameState ApplyAction(GameState state, CardDatabase.CardEntry card)
    {
        return state;  // 나중에 구현
    }
}
