public class CardInstance
{
    public CardData data;

    public int currentCost;

    public bool upgraded;
    public bool exhaust;
    public bool ethereal;

    public CardInstance(CardData cardData)
    {
        data = cardData;

        currentCost = data.cost;

        upgraded = false;

        exhaust = false;

        ethereal = false;
    }

    /// <summary>
    /// 배틀 씬에 처음 입장했을 때 덱에 있는 카드들을 드로우 파일에 넣을 때 사용
    /// </summary>
    /// <param name="original"></param>
    public CardInstance(CardInstance original)
    {
        data = original.data;

        currentCost = original.currentCost;
        upgraded = original.upgraded;
        exhaust = original.exhaust;
        ethereal = original.ethereal;
    }
}