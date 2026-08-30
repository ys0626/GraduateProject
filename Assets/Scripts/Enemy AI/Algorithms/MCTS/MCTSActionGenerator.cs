using System.Collections.Generic;

public static class MCTSActionGenerator
{
    public static List<MCTSAction> GetLegalActions(SimGameState state, TurnPlayHistory history)
    {
        bool pruningOn = SimulationManager.instance == null || SimulationManager.instance.EnableHeuristicPruning;

        HashSet<CardKey> unique = new HashSet<CardKey>();
        List<MCTSAction> actions = new List<MCTSAction>();

        foreach (var card in state.self.hand)
        {
            if (card.currentCost > state.self.CurrentEnergy)
                continue;

            CardData data = card.data;

            if (pruningOn)
            {
                if (history.hasDoubleTap && data.cardType != CardType.Attack)
                    continue;

                if (history.hasPlainAttack && (data.tags & CardTag.Debuff) != 0)
                    continue;

                if ((history.hasAnyAttack || history.hasLimitBreak) &&
                    (data.tags & CardTag.StrengthGain) != 0)
                    continue;

                if ((data.tags & CardTag.DoubleTap) != 0 &&
                    !HasFollowUpAttack(state, card))
                    continue;
            }

            CardKey key = CardKey.From(card);

            if (!unique.Add(key))
                continue;

            actions.Add(new MCTSAction { cardKey = key });
        }

        return actions;
    }

    private static bool HasFollowUpAttack(SimGameState state, CardInstance doubleTapCard)
    {
        int remainingEnergy = state.self.CurrentEnergy - doubleTapCard.currentCost;

        foreach (var other in state.self.hand)
        {
            if (other == doubleTapCard) continue;

            if (other.data.cardType == CardType.Attack &&
                other.currentCost <= remainingEnergy)
                return true;
        }

        return false;
    }
}

public struct MCTSAction
{
    public CardKey cardKey;
}