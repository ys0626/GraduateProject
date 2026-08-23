using System.Collections.Generic;

public static class MCTSActionGenerator
{
    public static List<MCTSAction> GetLegalActions(SimGameState state, TurnPlayHistory history)
    {
        HashSet<CardKey> unique = new HashSet<CardKey>();
        List<MCTSAction> actions = new List<MCTSAction>();

        foreach (var card in state.self.hand)
        {
            if (card.currentCost > state.self.CurrentEnergy)
                continue;

            CardData data = card.data;

            // DoubleTap 이후엔 반드시 공격 카드만 허용 =====
            if (history.hasDoubleTap && data.cardType != CardType.Attack)
                continue;

            // 순수 공격 이후 디버프 카드 배제 =====
            if (history.hasPlainAttack && (data.tags & CardTag.Debuff) != 0)
                continue;

            //  공격 또는 한계돌파 이후 힘 증가 카드 배제 =====
            if ((history.hasAnyAttack || history.hasLimitBreak) &&
                (data.tags & CardTag.StrengthGain) != 0)
                continue;

            //  DoubleTap 카드 자체는 뒤에 공격 카드가 있을 때만 후보로 =====
            if ((data.tags & CardTag.DoubleTap) != 0 &&
                !HasFollowUpAttack(state, card))
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