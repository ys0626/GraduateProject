using System.Collections.Generic;

public static class MCTSActionGenerator
{
    public static List<MCTSAction> GetLegalActions(SimGameState state)
    {
        HashSet<CardKey> unique = new HashSet<CardKey>();
        List<MCTSAction> actions = new List<MCTSAction>();

        foreach (var card in state.self.hand)
        {
            if (card.currentCost > state.self.CurrentEnergy)
                continue;

            CardKey key = CardKey.From(card);

            if (!unique.Add(key))
                continue;

            actions.Add(new MCTSAction
            {
                cardKey = key
            });
        }

        return actions;
    }
}

public struct MCTSAction
{
    public CardKey cardKey;
}