
// RelicEffectType을 실제 IRelicEffect 구현으로 연결하는 진입점
// 현재는 효과가 없으므로 null을 반환하며, 효과 제작 시 switch 문을 확장

public static class RelicEffectFactory
{
    public static IRelicEffect Create(RelicInstance relic)
    {
        if (relic == null || relic.Data == null)
        {
            return null;
        }

        // 아직 구현된 효과가 없으므로 None은 효과 객체를 만들지 않는다.
        // 새 효과를 만들 때 여기서 RelicEffectType별 구현 클래스를 생성한다.
        switch (relic.Data.EffectType)
        {
            case RelicEffectType.None:
            default:
                return null;
        }
    }
}
