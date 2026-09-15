
// RelicEffectType을 실제 IRelicEffect 구현으로 연결하는 진입점
// 현재는 효과가 없으므로 null을 반환하며, 효과 제작 시 switch 문을 확장

public static class RelicEffectFactory
{
    public static IRelicEffect Create(RelicInstance relic)
    {
        // TODO: relic.Data.EffectType별 구체 효과 객체를 생성
        return null;
    }
}
