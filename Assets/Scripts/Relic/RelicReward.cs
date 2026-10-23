
// 상점과 이벤트가 공통으로 사용하는 유물 보상 정보
// 구매 가격·선택지 표시·이벤트 결과는 이후 시스템에서 확장

public class RelicReward
{
    public RelicData Relic { get; }
    public RelicSource Source { get; }

    public RelicReward(RelicData relic, RelicSource source)
    {
        Relic = relic;
        Source = source;
    }

    // 보상을 실제 런 보유 목록에 지급
    public void Grant()
    {
        if (Relic == null)
        {
            UnityEngine.Debug.LogError("유물 보상 지급에 실패했습니다: RelicData가 없습니다.");
            return;
        }

        RelicManager.EnsureInstance().Acquire(Relic, Source);
    }
}
