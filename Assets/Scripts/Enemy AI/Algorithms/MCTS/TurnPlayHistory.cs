public struct TurnPlayHistory
{
    public bool hasPlainAttack;   // 디버프 없는 순수 공격 카드 사용함
    public bool hasAnyAttack;     // 공격 카드(종류 무관) 사용함
    public bool hasLimitBreak;    // 한계돌파 사용함
    public bool hasDoubleTap;     // DoubleTap 사용함

    public TurnPlayHistory Extend(CardData playedCard)
    {
        if (playedCard == null) return this;

        TurnPlayHistory next = this;

        bool isAttack = playedCard.cardType == CardType.Attack;
        bool isDebuff = (playedCard.tags & CardTag.Debuff) != 0;

        if (isAttack && !isDebuff) next.hasPlainAttack = true;
        if (isAttack) next.hasAnyAttack = true;
        if ((playedCard.tags & CardTag.LimitBreak) != 0) next.hasLimitBreak = true;
        if ((playedCard.tags & CardTag.DoubleTap) != 0) next.hasDoubleTap = true;

        return next;
    }
}