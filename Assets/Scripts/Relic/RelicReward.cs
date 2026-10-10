
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

    public void Grant()
    {
        if (RelicManager.Instance == null)
        {
            UnityEngine.Debug.LogError("RelicManager.Instance가 없습니다.");
            return;
        }

        if (Relic == null)
        {
            UnityEngine.Debug.LogError("지급하려는 RelicData가 null입니다.");
            return;
        }

        RelicManager.Instance.Acquire(Relic, Source);
    }
}
