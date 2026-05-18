public struct CardKey
{
    public CardData data;
    public int cost;
    public bool upgraded; 
    public bool exhaust;
    public bool ethereal;

    public override bool Equals(object obj)
    {
        if (!(obj is CardKey)) return false;

        CardKey other = (CardKey)obj;

        return data == other.data &&
               cost == other.cost &&
               upgraded == other.upgraded&&
               exhaust == other.exhaust&&
               ethereal == other.ethereal;
    }

    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 31 + data.GetHashCode();
        hash = hash * 31 + cost;
        hash = hash * 31 + upgraded.GetHashCode();
        hash = hash * 31 + exhaust.GetHashCode();
        hash = hash * 31 + ethereal.GetHashCode();

        return hash;
    }

    public static CardKey From(CardInstance card)
    {
        return new CardKey
        {
            data = card.data,
            cost = card.currentCost,
            upgraded = card.upgraded,
            exhaust = card.exhaust,
            ethereal = card.ethereal
        };
    }
}