//현재 런에서 플레이어가 실제로 보유한 유물

public class RelicInstance
{
    public RelicData Data { get; }

    // 유물 데이터를 바탕으로 런타임 인스턴스 생성

    public RelicInstance(RelicData data)
    {
        Data = data;
    }
}
