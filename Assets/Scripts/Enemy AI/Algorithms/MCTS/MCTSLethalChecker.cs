using System.Collections.Generic;
public static class MCTSLethalChecker
{
    public static List<MCTSAction> FindLethalSequence(SimGameState rootState)
    {
        return SearchLethal(rootState.Clone(), default);
    }

    private static List<MCTSAction> SearchLethal(SimGameState state, TurnPlayHistory history)
    {
        List<MCTSAction> legalActions = MCTSActionGenerator.GetLegalActions(state, history);

        foreach (MCTSAction action in legalActions)
        {
            SimGameState next = state.Clone();
            TurnPlayHistory nextHistory = history.Extend(action.cardKey.data);

            MCTSExpansion.ApplyAction(next, action);

            if (next.opponent.CurrentHP <= 0)
            {
                return new List<MCTSAction> { action };
            }

            List<MCTSAction> deeper = SearchLethal(next, nextHistory);

            if (deeper != null)
            {
                deeper.Insert(0, action);
                return deeper;
            }
        }

        return null;
    }
}