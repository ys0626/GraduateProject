/// <summary>
/// 런타임에서 사용하는 카드 인스턴스
/// CardData는 공유 원본, CardInstance는 개별 복사본
/// </summary>
public class CardInstance
{
    public CardData data;        // CardData 원본 참조

    public int currentCost;     // 런타임 비용 (비용 감소 효과 대응)
    public bool upgraded;        // 강화 여부
    public bool exhaust;         // 소멸 카드 여부 (런타임)
    public bool ethereal;        // 턴 종료 시 소멸 여부 (런타임)

    // CardData로 생성
    public CardInstance(CardData cardData)
    {
        data = cardData;
        currentCost = data.cost;
        upgraded = false;
        exhaust = false;
        ethereal = false;
    }

    // 복사 생성자 (DeckManager.InitializeDeck에서 사용)
    public CardInstance(CardInstance original)
    {
        data = original.data;
        currentCost = original.currentCost;
        upgraded = original.upgraded;
        exhaust = original.exhaust;
        ethereal = original.ethereal;
    }
}
