
// 모든 유물 효과가 구현할 전투·런 진행 훅
// 지금은 계약만 정의, 구체 효과는 나중에 IRelicEffect 구현 클래스로

public interface IRelicEffect
{
    //유물을 획득한 직후 한 번 호출
    void OnAcquired(RelicInstance relic);

    //전투 시작 시 호출
    void OnBattleStarted(Entity owner, Entity opponent);

    //보유자의 턴 시작 시 호출
    void OnTurnStarted(Entity owner);

    //카드 사용이 성공한 뒤 호출
    void OnCardPlayed(Entity owner, CardInstance card, Entity target);

    //보유자의 턴 종료 시 호출
    void OnTurnEnded(Entity owner);

    //전투 종료 시 호출
    void OnBattleEnded(bool playerWon);
}
